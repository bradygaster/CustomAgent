using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using System.ClientModel;
using System.Text.Json;

// Configuration properties
string AgentName = "Custom AI Agent";
string DomainName = "the specified domain";
string ToneStyle = "scholarly but approachable";
float Temperature = 0.1f;
float TopP = 0.1f;
int MaxCompletionTokens = 4096;
int MaxPromptTokens = 8192;
string WelcomeMessage = "AI Agent Console";
string PromptMessage = "Ask a question about the subject (type 'exit' to quit, 'save' to save the conversation):";

// Load configuration
var builder = new ConfigurationBuilder()
    .AddUserSecrets<Program>();

var configuration = builder.Build();

// Get connection details from configuration
string apiDeploymentName = configuration["Azure:ModelName"] ?? throw new InvalidOperationException("Azure:ModelName is not set in the configuration. Use 'dotnet user-secrets set \"Azure:ModelName\" \"your-model-name\"'");
string endpoint = configuration["Azure:Endpoint"] ?? throw new InvalidOperationException("Azure:Endpoint is not set in the configuration. Use 'dotnet user-secrets set \"Azure:Endpoint\" \"your-endpoint\"'");

// Load optional configuration with defaults
AgentName = configuration["Agent:Name"] ?? AgentName;
DomainName = configuration["Agent:Domain"] ?? DomainName;
ToneStyle = configuration["Agent:ToneStyle"] ?? ToneStyle;
Temperature = ParseFloat(configuration["Agent:Temperature"], Temperature);
TopP = ParseFloat(configuration["Agent:TopP"], TopP);
MaxCompletionTokens = ParseInt(configuration["Agent:MaxCompletionTokens"], MaxCompletionTokens);
MaxPromptTokens = ParseInt(configuration["Agent:MaxPromptTokens"], MaxPromptTokens);
WelcomeMessage = configuration["UI:WelcomeMessage"] ?? WelcomeMessage;
PromptMessage = configuration["UI:PromptMessage"] ?? PromptMessage;

Console.WriteLine(WelcomeMessage);
Console.WriteLine(new string('-', WelcomeMessage.Length));

// Create the agent client
PersistentAgentsClient agentClient = new(endpoint, new DefaultAzureCredential());

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
    // Create the agent
    PersistentAgent agent = await agentClient.Administration.CreateAgentAsync(
        model: apiDeploymentName,
        name: AgentName,
        instructions: combinedInstructions,
        temperature: Temperature
    );

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Agent created with ID: {agent.Id}");
    Console.ResetColor();

    // Create a thread for the conversation
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("Creating conversation thread...");
    Console.ResetColor();

    PersistentAgentThread thread = await agentClient.Threads.CreateThreadAsync();

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Thread created with ID: {thread.Id}");
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
        Console.WriteLine($"Saving thread with ID: {thread.Id} for agent ID: {agent.Id}. You can view this in AI Foundry at https://ai.azure.com");
        Console.ResetColor();
        keepRunning = false;
        continue;
    }

    // Send the user message to the thread
    await agentClient.Messages.CreateMessageAsync(
        threadId: thread.Id,
        role: MessageRole.User,
        content: prompt
    );    // Start the agent's run on the thread
    AsyncCollectionResult<StreamingUpdate> streamingUpdate = agentClient.Runs.CreateRunStreamingAsync(
        threadId: thread.Id,
        agentId: agent.Id,
        maxCompletionTokens: MaxCompletionTokens,
        maxPromptTokens: MaxPromptTokens,
        temperature: Temperature,
        topP: TopP
    );

    // Handle the streaming response
    await foreach (StreamingUpdate update in streamingUpdate)
    {
        await HandleStreamingUpdateAsync(update);
    }
}

// Clean up resources if not saving
if (keepRunning)
{
    await agentClient.Threads.DeleteThreadAsync(thread.Id);
    await agentClient.Administration.DeleteAgentAsync(agent.Id);
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

// Handler for streaming updates
async Task HandleStreamingUpdateAsync(StreamingUpdate update)
{
    switch (update.UpdateKind)
    {
        case StreamingUpdateReason.MessageUpdated:
            // The agent has a response to the user
            MessageContentUpdate messageContentUpdate = (MessageContentUpdate)update;
            Console.ForegroundColor = ConsoleColor.White;
            await Console.Out.WriteAsync(messageContentUpdate.Text);
            Console.ResetColor();
            break;

        case StreamingUpdateReason.RunCompleted:
            // The run is complete, so we can print a new line
            await Console.Out.WriteLineAsync();
            break;

        case StreamingUpdateReason.RunFailed:
            // The run failed, so we can print the error message
            RunUpdate runFailedUpdate = (RunUpdate)update;
            Console.ForegroundColor = ConsoleColor.Red;
            await Console.Out.WriteLineAsync($"Error: {runFailedUpdate.Value.LastError.Message} (code: {runFailedUpdate.Value.LastError.Code})");
            Console.ResetColor();
            break;
    }
}
