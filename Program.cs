using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.Text.Json;

// Load configuration
var builder = new ConfigurationBuilder().AddUserSecrets<Program>();
var configuration = builder.Build();

// Load configuration with defaults
string AgentName = configuration["Agent:Name"] ?? "Custom AI Agent";
string DomainName = configuration["Agent:Domain"] ?? "the specified domain";
string ToneStyle = configuration["Agent:ToneStyle"] ?? "scholarly but approachable";
float Temperature = ParseFloat(configuration["Agent:Temperature"], 0.1f);
float TopP = ParseFloat(configuration["Agent:TopP"], 0.1f);
int MaxCompletionTokens = ParseInt(configuration["Agent:MaxCompletionTokens"], 4096);
int MaxPromptTokens = ParseInt(configuration["Agent:MaxPromptTokens"], 8192);
string WelcomeMessage = configuration["UI:WelcomeMessage"] ?? "AI Agent Console";
string PromptMessage = configuration["UI:PromptMessage"] ?? "Ask a question about the subject (type 'exit' to quit, 'save' to save the conversation):";

// Get connection details from configuration
string apiDeploymentName = configuration["Azure:ModelName"] ?? throw new InvalidOperationException("Azure:ModelName is not set in the configuration. Use 'dotnet user-secrets set \"Azure:ModelName\" \"your-model-name\"'");
string endpoint = configuration["Azure:Endpoint"] ?? throw new InvalidOperationException("Azure:Endpoint is not set in the configuration. Use 'dotnet user-secrets set \"Azure:Endpoint\" \"your-endpoint\"'");

Console.WriteLine(WelcomeMessage);
Console.WriteLine(new string('-', WelcomeMessage.Length));

// Create the Azure OpenAI client with managed identity authentication
var azureOpenAIClient = new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential());

// Get the chat client for the specified model
var chatClient = azureOpenAIClient.GetChatClient(apiDeploymentName);

// Define file paths
string promptsFolderPath = Path.Combine(AppContext.BaseDirectory, "prompts");
string instructionsFolderPath = Path.Combine(AppContext.BaseDirectory, "instructions");
string promptTemplatePath = Path.Combine(promptsFolderPath, "prompt_template.md");

// Get all instruction files
List<string> instructionFiles = Directory.GetFiles(instructionsFolderPath, "*.md").ToList();

// Check if files exist
if (instructionFiles.Count == 0)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("No instruction files found. Ensure at least one .md file exists in the instructions directory.");
    Console.ResetColor();
    return;
}

if (!File.Exists(promptTemplatePath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Prompt template file not found. Ensure prompt_template.md exists in the prompts directory.");
    Console.ResetColor();
    return;
}

// Load instruction content
List<string> contentList = new();
foreach (var file in instructionFiles)
{
    string content = File.ReadAllText(file);
    string fileName = Path.GetFileNameWithoutExtension(file);
    contentList.Add($"## {fileName}\n{content}");
}
string instructionsContent = string.Join("\n\n", contentList);

// Load and customize prompt template
string promptTemplate = File.ReadAllText(promptTemplatePath);
string customizedPrompt = promptTemplate
    .Replace("{{DOMAIN_NAME}}", DomainName)
    .Replace("{{TONE_STYLE}}", ToneStyle);

// Debug output to verify files were loaded correctly
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"Successfully loaded {instructionFiles.Count} instruction files from instructions folder");
Console.WriteLine($"Successfully loaded prompt template from prompts folder");
Console.ResetColor();

// Combine the instructions and prompt
string combinedInstructions = $"{customizedPrompt}\n\n# Knowledge Base\n{instructionsContent}";

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("Creating agent...");
Console.ResetColor();

try
{
    // Create the agent using Microsoft Agent Framework
    AIAgent agent = chatClient.CreateAIAgent(
        name: AgentName,
        instructions: combinedInstructions
    );

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Agent created successfully: {AgentName}");
    Console.ResetColor();

    // Get a new thread for conversation management
    AgentThread thread = agent.GetNewThread();

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Thread created for conversation");
    Console.ResetColor();

    // Start conversation loop
    bool keepRunning = true;
    while (keepRunning)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n{PromptMessage}");
        Console.ResetColor();
        
        string? prompt = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(prompt))
        {
            continue;
        }

        if (prompt.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            break;
        }

        if (prompt.Equals("save", StringComparison.OrdinalIgnoreCase))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Conversation saved in thread. You can continue the conversation later.");
            Console.ResetColor();
            keepRunning = false;
            continue;
        }

        try
        {
            // Use streaming response from the agent
            await foreach (var update in agent.RunStreamingAsync(prompt, thread))
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(update.Text);
                Console.ResetColor();
            }
            Console.WriteLine(); // Add a newline after the response
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nError during agent run: {ex.Message}");
            Console.ResetColor();
        }
    }

}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\nError: {ex.Message}");
    Console.WriteLine("\nIt appears you haven't provisioned the required Azure resources yet.");
    Console.WriteLine("To provision resources, run the following command:");
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\n    .\\infra\\up.ps1\n");
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Please ensure you have the necessary Azure permissions and credentials configured.");
    Console.ResetColor();
    return;
}

// Helper methods for configuration parsing
static float ParseFloat(string? value, float defaultValue)
{
    if (string.IsNullOrEmpty(value) || !float.TryParse(value, out float result))
    {
        return defaultValue;
    }
    return result;
}

static int ParseInt(string? value, int defaultValue)
{
    if (string.IsNullOrEmpty(value) || !int.TryParse(value, out int result))
    {
        return defaultValue;
    }
    return result;
}
