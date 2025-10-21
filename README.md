# Custom AI Agent Framework

This repository contains a flexible AI agent framework built using Microsoft's Agents AI framework. The agent can be configured to specialize in any knowledge domain, allowing you to create custom AI assistants with specific expertise.

## Project Structure

- `Program.cs` - The main entry point using Host builder pattern
- `ConversationLoop.cs` - Handles the interactive chat loop with the AI agent
- `Extensions.cs` - Dependency injection configuration and service registration
- `Settings.cs` - Configuration classes for agent, UI, and Azure settings
- `ConsoleClient.cs` - Console output utilities for colored text display
- `InstructionLoader.cs` - Loads and processes instruction files and prompt templates
- `instructions/` - Directory containing the agent's knowledge base
  - `jabberwocky.md` - Sample knowledge about the Jabberwocky
  - `quantum.txt` - Sample knowledge about quantum computing
- `prompts/` - Directory containing prompt templates
  - `prompt_template.md` - Configurable template for defining agent behaviors
- `infra/` - Infrastructure as Code for Azure deployment
  - `up.ps1` - PowerShell script to provision Azure resources
  - `down.ps1` - PowerShell script to tear down Azure resources
  - Various Bicep files for Azure resource definitions

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- Azure subscription with appropriate permissions
- Azure CLI installed and configured
- PowerShell 7+ for running deployment scripts

## Key Dependencies

The application uses the following key NuGet packages:
- `Microsoft.Agents.AI.OpenAI` (v1.0.0-preview.251009.1) - Microsoft's Agents AI framework
- `Azure.AI.OpenAI` (v2.1.0) - Azure OpenAI integration
- `Azure.Identity` (v1.13.2) - Azure authentication
- `Microsoft.Extensions.Hosting` (v9.0.10) - .NET Host builder pattern

## Setup Instructions

### 1. Clone the Repository

```powershell
git clone https://github.com/yourusername/CustomAgent.git
cd CustomAgent
```

### 2. Provision Azure Resources

Run the provisioning script to deploy necessary Azure resources and configure the application:

```powershell
.\infra\up.ps1
```

This script will:
- Create Azure AI Foundry resources including a GPT-4o model deployment
- Set up required IAM permissions (Azure AI Developer role)
- Configure the application's user secrets with proper Azure connection details
- Set default agent configuration for the Jabberwocky domain

The script automatically configures these user secrets:
- `Azure:ModelName` - Set to "gpt-4o"
- `Azure:Endpoint` - The Azure AI Projects endpoint URL
- Default agent settings for the Jabberwocky domain

### 3. Build and Run the Application

```powershell
dotnet build
dotnet run
```

## Sample Interaction

Here's what a typical interaction with the agent looks like:

```
Jabberwocky AI Agent

Ask a question about the Jabberwocky (type 'exit' to quit, 'save' to save the conversation):
What can you tell me about the Jabberwocky?

Based on my knowledge base, the Jabberwocky is a legendary creature that originated in Lewis Carroll's novel "Through the Looking-Glass, and What Alice Found There" (1871). The creature is the subject of the famous nonsensical poem "Jabberwocky," which is considered one of the greatest nonsense poems written in the English language...

Ask a question about the Jabberwocky (type 'exit' to quit, 'save' to save the conversation):
exit
```

The application provides streaming responses from the AI agent, displaying text as it's generated.

## Configuration

The application uses .NET's configuration system with user secrets for sensitive data. Configuration is organized into three main sections:

### Azure Settings (Required)

These are automatically set by the `up.ps1` script:

```powershell
dotnet user-secrets set "Azure:ModelName" "gpt-4o"
dotnet user-secrets set "Azure:Endpoint" "your-azure-endpoint"
```

### Agent Settings (Optional)

```powershell
dotnet user-secrets set "Agent:Name" "Your Agent Name"
dotnet user-secrets set "Agent:Domain" "your specialized domain name"
dotnet user-secrets set "Agent:ToneStyle" "scholarly but approachable"
```

### UI Settings (Optional)

```powershell
dotnet user-secrets set "UI:WelcomeMessage" "Your Custom Welcome Message"
dotnet user-secrets set "UI:PromptMessage" "Your custom prompt message"
```

## Files Structure

### Instructions Files

The `instructions/` directory contains markdown and text files that form the agent's knowledge base. Each file will be loaded and combined into the agent's instructions. The repository includes two example instruction files:

- `instructions/jabberwocky.md` - Comprehensive information about the Jabberwocky creature and poem
- `instructions/quantum.txt` - Basic introduction to quantum computing concepts

All `.md` files in the instructions directory are automatically loaded by the `InstructionLoader` class.

### Prompt Template

The `prompts/prompt_template.md` file defines the agent's behavior and response guidelines. It uses placeholders that are replaced with configuration values:

- `{{DOMAIN_NAME}}` - Replaced with the Agent:Domain setting
- `{{TONE_STYLE}}` - Replaced with the Agent:ToneStyle setting

This template system allows you to create specialized agents without modifying the code.

## Customizing Content

The application comes with sample content about the Jabberwocky and quantum computing. To customize it for your own domain:

1. **Replace instruction files**: Add your own `.md` or `.txt` files to the `instructions/` directory
2. **Update the prompt template**: Modify `prompts/prompt_template.md` to define your agent's behavior
3. **Configure the agent**: Update the default settings in `infra/up.ps1` or use user secrets:

```powershell
dotnet user-secrets set "Agent:Name" "Your Expert Name"
dotnet user-secrets set "Agent:Domain" "your specialized domain"
dotnet user-secrets set "Agent:ToneStyle" "your preferred tone"
dotnet user-secrets set "UI:WelcomeMessage" "Your Custom Welcome"
dotnet user-secrets set "UI:PromptMessage" "Your custom prompt:"
```

The `InstructionLoader` class automatically processes all instruction files and combines them with the prompt template to create the agent's complete instructions.

## Architecture

The application follows modern .NET patterns:

- **Host Builder Pattern**: Uses `Microsoft.Extensions.Hosting` for dependency injection and configuration
- **Streaming Responses**: Implements real-time streaming of AI responses using `RunStreamingAsync`
- **Azure Authentication**: Uses `DefaultAzureCredential` for seamless Azure authentication
- **Configuration Management**: Leverages .NET's configuration system with user secrets
- **Modular Design**: Separates concerns across multiple classes for maintainability

The main flow:
1. `Program.cs` sets up the host and starts the conversation
2. `Extensions.cs` configures services and creates the AI agent
3. `InstructionLoader.cs` processes knowledge base files
4. `ConversationLoop.cs` handles the interactive chat experience
5. `ConsoleClient.cs` provides colored console output

## Error Handling

The application includes comprehensive error handling:

- Validates required Azure configuration at startup
- Checks for the existence of prompt and instruction files
- Gracefully handles agent communication errors
- Provides clear error messages with colored console output

Common error scenarios:
- Missing Azure configuration: "Azure:ModelName not configured"
- Missing files: "Prompt template file not found"
- Agent communication issues: Displayed during chat interaction

## Cleanup

When you're done with the application, you can remove the Azure resources to avoid incurring costs:

```powershell
.\infra\down.ps1
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
