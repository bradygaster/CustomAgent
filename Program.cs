using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using System.ClientModel;
using System.Text.Json;

// Load configuration
var builder = new ConfigurationBuilder()
    .AddUserSecrets<Program>();

var configuration = builder.Build();

// Get connection details from configuration
string apiDeploymentName = configuration["Azure:ModelName"] ?? throw new InvalidOperationException("Azure:ModelName is not set in the configuration. Use 'dotnet user-secrets set \"Azure:ModelName\" \"your-model-name\"'");
string endpoint = configuration["Azure:Endpoint"] ?? throw new InvalidOperationException("Azure:Endpoint is not set in the configuration. Use 'dotnet user-secrets set \"Azure:Endpoint\" \"your-endpoint\"'");

Console.WriteLine("Jabberwocky AI Agent");
Console.WriteLine("------------------");

// Create the agent client
PersistentAgentsClient agentClient = new(endpoint, new DefaultAzureCredential());

// Load instructions files
string knowledgeFolderPath = Path.Combine(AppContext.BaseDirectory, "knowledge");
string instructionsPath = Path.Combine(knowledgeFolderPath, "instructions.md");
string promptPath = Path.Combine(knowledgeFolderPath, "prompt.md");

if (!File.Exists(instructionsPath) || !File.Exists(promptPath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Instructions or prompt file not found. Ensure both files exist in the knowledge directory.");
    Console.ResetColor();
    return;
}

string jabberwockyInfo = File.ReadAllText(instructionsPath);
string agentPrompt = File.ReadAllText(promptPath);

// Debug output to verify files were loaded correctly
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"Successfully loaded instructions ({jabberwockyInfo.Length} chars) and prompt ({agentPrompt.Length} chars) from knowledge folder");
Console.ResetColor();

// Combine the instructions and prompt
string combinedInstructions = $"{agentPrompt}\n\n# Knowledge Base\n{jabberwockyInfo}";

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("Creating agent...");
Console.ResetColor();

try
{
    // Create the agent
    PersistentAgent agent = await agentClient.Administration.CreateAgentAsync(
        model: apiDeploymentName,
        name: "Jabberwocky Expert",
        instructions: combinedInstructions,
        temperature: 0.1f
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
    Console.WriteLine("\nAsk a question about the Jabberwocky (type 'exit' to quit, 'save' to save the conversation):");
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
    );

    // Start the agent's run on the thread
    AsyncCollectionResult<StreamingUpdate> streamingUpdate = agentClient.Runs.CreateRunStreamingAsync(
        threadId: thread.Id,
        agentId: agent.Id,
        maxCompletionTokens: 4096,
        maxPromptTokens: 8192,
        temperature: 0.1f,
        topP: 0.1f
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
