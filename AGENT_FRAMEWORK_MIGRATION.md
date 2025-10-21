# Migration to Microsoft Agent Framework

This branch (`agent-framework`) contains the migrated version of the CustomAgent project using the new Microsoft Agent Framework instead of the legacy Azure AI Persistent Agents.

## What Changed

### 1. **Package References Updated**
- **Removed**: `Azure.AI.Agents.Persistent`
- **Added**: 
  - `Microsoft.Agents.AI.OpenAI` (the core Agent Framework package)
  - `Azure.AI.OpenAI` (updated Azure OpenAI client)
  - `OpenAI` (OpenAI client dependency)

### 2. **Code Architecture Improvements**
- **Simplified Agent Creation**: No more complex persistent agent management
- **Better Error Handling**: More streamlined exception handling
- **Improved Streaming**: Native streaming support with async enumerable
- **Managed Identity**: Using `DefaultAzureCredential` for better security
- **Thread Management**: Simplified conversation thread handling

### 3. **Key Benefits of Migration**

#### **Simplified API**
```csharp
// OLD: Complex persistent agent setup
PersistentAgentsClient agentClient = new(endpoint, new DefaultAzureCredential());
PersistentAgent agent = await agentClient.Administration.CreateAgentAsync(...);
PersistentAgentThread thread = await agentClient.Threads.CreateThreadAsync();

// NEW: Simple agent creation
var chatClient = new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
    .GetChatClient(modelName);
AIAgent agent = chatClient.CreateAIAgent(name: AgentName, instructions: instructions);
AgentThread thread = agent.GetNewThread();
```

#### **Better Streaming**
```csharp
// OLD: Complex streaming with manual update handling
AsyncCollectionResult<StreamingUpdate> streamingUpdate = agentClient.Runs.CreateRunStreamingAsync(...);
await foreach (StreamingUpdate update in streamingUpdate)
{
    await HandleStreamingUpdateAsync(update); // Custom handler needed
}

// NEW: Simple streaming with direct text output
await foreach (var update in agent.RunStreamingAsync(prompt, thread))
{
    Console.Write(update.Text);
}
```

#### **Enhanced Security**
- Uses `DefaultAzureCredential` which automatically handles:
  - Managed Identity (when running on Azure)
  - Azure CLI credentials (local development)
  - Visual Studio credentials
  - Environment variables
  - And more...

#### **Framework Advantages**
- **Cross-provider support**: Can easily switch between Azure OpenAI, OpenAI, and other providers
- **Workflow capabilities**: Ready for future multi-agent workflows
- **Middleware support**: Add logging, monitoring, and custom processing
- **Tool integration**: Better support for function calling and external tools
- **Observability**: Built-in OpenTelemetry support
- **Type safety**: Strong typing throughout the API

## Configuration Changes

The configuration remains largely the same, but with some improvements:

### Required Settings (unchanged)
```powershell
dotnet user-secrets set "Azure:ModelName" "your-model-name"
dotnet user-secrets set "Azure:Endpoint" "your-endpoint"
```

### Optional Settings (unchanged)
All existing optional settings continue to work exactly the same way.

## Deployment

The infrastructure scripts (`infra/up.ps1` and `infra/down.ps1`) work with both versions since they provision the same Azure resources. The Agent Framework can use the same Azure OpenAI endpoints.

## Future Enhancements Enabled

With this migration, the project is now ready for advanced features:

1. **Multi-Agent Workflows**: Orchestrate multiple specialized agents
2. **Middleware**: Add logging, monitoring, validation, etc.
3. **Tool Integration**: Enhanced function calling and external API integration
4. **Observability**: Built-in telemetry and monitoring
5. **Context Providers**: Advanced memory and state management
6. **Cross-Provider Support**: Use OpenAI, Anthropic, or other providers

## Backward Compatibility

While the internal implementation has changed significantly, the user experience remains virtually identical:
- Same command-line interface
- Same configuration options
- Same conversation flow
- Same save/exit functionality

## Testing the Migration

To test the migrated version:

1. Make sure you're on the `agent-framework` branch
2. Restore packages: `dotnet restore`
3. Build: `dotnet build`
4. Run: `dotnet run`

The application should behave exactly the same as before, but with improved performance and reliability.

## Migration Benefits Summary

✅ **Simplified codebase** - Fewer lines of code, easier to maintain  
✅ **Better performance** - More efficient streaming and API calls  
✅ **Enhanced security** - Improved credential management  
✅ **Future-ready** - Access to latest Agent Framework features  
✅ **Better error handling** - More robust exception management  
✅ **Improved developer experience** - Cleaner APIs and better documentation  

This migration positions the project to take advantage of the latest developments in AI agent technology while maintaining full compatibility with existing functionality.