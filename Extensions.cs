using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI;

namespace Microsoft.Extensions.Hosting;

internal static class Extensions
{
    public static IHostApplicationBuilder AddSettings(this IHostApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        builder.Configuration.AddUserSecrets<Program>();
        builder.Services.Configure<AppSettings>(builder.Configuration);
        builder.Services.Configure<AzureSettings>(builder.Configuration.GetSection("Azure"));
        return builder;
    }

    public static IHostApplicationBuilder AddServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ConsoleClient>();
        builder.Services.AddSingleton<InstructionLoader>();
        builder.Services.AddSingleton<ConversationLoop>(services =>
        {
            var appOptions = services.GetRequiredService<IOptions<AppSettings>>().Value;
            var azureOptions = services.GetRequiredService<IOptions<AzureSettings>>().Value;
            var instructionLoader = services.GetRequiredService<InstructionLoader>();
            var consoleClient = services.GetRequiredService<ConsoleClient>();

            // Validate required Azure settings
            if (string.IsNullOrWhiteSpace(azureOptions.ModelName))
                throw new InvalidOperationException("Azure:ModelName not configured.");
            if (string.IsNullOrWhiteSpace(azureOptions.Endpoint))
                throw new InvalidOperationException("Azure:Endpoint not configured.");

            // Load instruction data with metadata
            var instructionData = instructionLoader.LoadInstruction(appOptions.InstructionFile);

            // Create a new AI Agent from an Azure Open AI Client
            var agent = new AzureOpenAIClient(new Uri(azureOptions.Endpoint), new DefaultAzureCredential())
                .GetChatClient(azureOptions.ModelName)
                .CreateAIAgent(
                    name: instructionData.Metadata.Name,
                    instructions: instructionData.Content
                );

            // Start conversation
            var thread = agent.GetNewThread();

            // Create a conversation loop, 1 per app run
            return new ConversationLoop(agent, thread, consoleClient, instructionData.Metadata);
        });

        return builder;
    }
}
