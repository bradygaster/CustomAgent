using Microsoft.Agents.AI;

public class ConversationLoop(AIAgent agent, AgentThread thread, ConsoleClient consoleClient, UISettings uiSettings)
{
    public async Task Chat()
    {
        consoleClient.Print(uiSettings.WelcomeMessage, ConsoleColor.Cyan);

        if (agent == null || thread == null)
            throw new InvalidOperationException("ConversationLoop not initialized with an agent and thread.");

        while (true)
        {
            consoleClient.Print($"\n{uiSettings.PromptMessage}", ConsoleColor.Yellow);
            var input = Console.ReadLine();

            switch (input?.Trim().ToLowerInvariant())
            {
                case null or "":
                    continue;
                case "exit":
                    return;
                default:
                    await ProcessAgentResponse(input);
                    break;
            }
        }
    }

    private async Task ProcessAgentResponse(string input)
    {
        try
        {
            await foreach (var update in agent!.RunStreamingAsync(input, thread!))
            {
                consoleClient.Fragment(update.Text, ConsoleColor.White);
            }
        }
        catch (Exception ex)
        {
            consoleClient.Print($"\nError during agent run: {ex.Message}", ConsoleColor.Red);
        }
    }
}