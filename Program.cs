using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;

// Load configuration
var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

// Configuration settings with defaults
var settings = new
{
    Agent = new
    {
        Name = configuration["Agent:Name"] ?? "Custom AI Agent",
        Domain = configuration["Agent:Domain"] ?? "the specified domain", 
        ToneStyle = configuration["Agent:ToneStyle"] ?? "scholarly but approachable"
    },
    UI = new
    {
        WelcomeMessage = configuration["UI:WelcomeMessage"] ?? "AI Agent Console",
        PromptMessage = configuration["UI:PromptMessage"] ?? "Ask a question about the subject (type 'exit' to quit, 'save' to save the conversation):"
    },
    Azure = new
    {
        ModelName = configuration["Azure:ModelName"] ?? throw new InvalidOperationException("Azure:ModelName not configured. Run: dotnet user-secrets set \"Azure:ModelName\" \"your-model-name\""),
        Endpoint = configuration["Azure:Endpoint"] ?? throw new InvalidOperationException("Azure:Endpoint not configured. Run: dotnet user-secrets set \"Azure:Endpoint\" \"your-endpoint\"")
    }
};

WriteStyledLine(settings.UI.WelcomeMessage, ConsoleColor.Cyan);
Console.WriteLine(new string('-', settings.UI.WelcomeMessage.Length));

try
{
    // Create Azure OpenAI client and agent
    var agent = new AzureOpenAIClient(new Uri(settings.Azure.Endpoint), new DefaultAzureCredential())
        .GetChatClient(settings.Azure.ModelName)
        .CreateAIAgent(name: settings.Agent.Name, instructions: LoadInstructions(settings.Agent.Domain, settings.Agent.ToneStyle));

    WriteStyledLine($"Agent created successfully: {settings.Agent.Name}", ConsoleColor.Green);

    // Start conversation
    var thread = agent.GetNewThread();
    WriteStyledLine("Thread created for conversation", ConsoleColor.Green);

    await RunConversationLoop(agent, thread, settings.UI.PromptMessage);
}
catch (Exception ex)
{
    WriteStyledLine($"\nError: {ex.Message}", ConsoleColor.Red);
    WriteStyledLine("\nIt appears you haven't provisioned the required Azure resources yet.", ConsoleColor.Red);
    WriteStyledLine("To provision resources, run the following command:", ConsoleColor.Red);
    WriteStyledLine("\n    .\\infra\\up.ps1\n", ConsoleColor.Yellow);
    WriteStyledLine("Please ensure you have the necessary Azure permissions and credentials configured.", ConsoleColor.Red);
}

// Helper methods
static void WriteStyledLine(string text, ConsoleColor color)
{
    Console.ForegroundColor = color;
    Console.WriteLine(text);
    Console.ResetColor();
}

static string LoadInstructions(string domainName, string toneStyle)
{
    var basePath = AppContext.BaseDirectory;
    var promptsPath = Path.Combine(basePath, "prompts", "prompt_template.md");
    var instructionsPath = Path.Combine(basePath, "instructions");

    // Validate required files exist
    if (!File.Exists(promptsPath))
        throw new FileNotFoundException("Prompt template file not found. Ensure prompt_template.md exists in the prompts directory.");

    var instructionFiles = Directory.GetFiles(instructionsPath, "*.md");
    if (instructionFiles.Length == 0)
        throw new FileNotFoundException("No instruction files found. Ensure at least one .md file exists in the instructions directory.");

    // Load and process instructions
    var promptTemplate = File.ReadAllText(promptsPath)
        .Replace("{{DOMAIN_NAME}}", domainName)
        .Replace("{{TONE_STYLE}}", toneStyle);

    var instructionsContent = string.Join("\n\n", 
        instructionFiles.Select(file => 
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetFileNameWithoutExtension(file);
            return $"## {fileName}\n{content}";
        }));

    WriteStyledLine($"Successfully loaded {instructionFiles.Length} instruction files and prompt template", ConsoleColor.Cyan);

    return $"{promptTemplate}\n\n# Knowledge Base\n{instructionsContent}";
}

static async Task RunConversationLoop(AIAgent agent, AgentThread thread, string promptMessage)
{
    while (true)
    {
        WriteStyledLine($"\n{promptMessage}", ConsoleColor.Yellow);
        var input = Console.ReadLine();

        switch (input?.Trim().ToLowerInvariant())
        {
            case null or "":
                continue;
            case "exit":
                return;
            case "save":
                WriteStyledLine("Conversation saved in thread. You can continue the conversation later.", ConsoleColor.Green);
                return;
            default:
                await ProcessAgentResponse(agent, thread, input);
                break;
        }
    }
}

static async Task ProcessAgentResponse(AIAgent agent, AgentThread thread, string input)
{
    try
    {
        await foreach (var update in agent.RunStreamingAsync(input, thread))
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(update.Text);
            Console.ResetColor();
        }
        Console.WriteLine();
    }
    catch (Exception ex)
    {
        WriteStyledLine($"\nError during agent run: {ex.Message}", ConsoleColor.Red);
    }
}
