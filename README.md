# Custom AI Agent Framework

This repository contains a flexible AI agent framework built using Azure's Persistent Agent capabilities. The agent can be configured to specialize in any knowledge domain, allowing you to create custom AI assistants with specific expertise.

## Project Structure

- `Program.cs` - The main C# application that creates and interacts with the AI agent
- `instructions/` - Directory containing the agent's knowledge base as markdown files
  - `*.md` - Markdown files containing domain-specific knowledge
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
- Create Azure AI Foundry resources
- Create model deployments
- Set up required IAM permissions
- Configure the application's user secrets with proper Azure connection details

The `up.ps1` script automatically sets the required user secrets, so you don't need to manually configure:
- `Azure:ModelName` - The name of the AI model for the agent
- `Azure:Endpoint` - The Azure endpoint URL for accessing the agent service

### 2. Provision Resources

When you run the provisioning script, it will:

1. Create Azure resources needed for the agent
2. Verify that the prompt and instruction files exist
3. Configure default settings for the agent

```powershell
.\infra\up.ps1
```

Make sure you have the necessary files in the `prompts` and `instructions` directories before running the script.

### 3. Build and Run the Application

```powershell
dotnet build
dotnet run
```

## Sample Interaction

Here's what a typical interaction with the agent looks like:

```
AI Agent Console
---------------
Successfully loaded 2 instruction files from instructions folder
Successfully loaded prompt template from prompts folder
Creating agent...
Agent created with ID: agent_abc123xyz789
Creating conversation thread...
Thread created with ID: thread_456def789ghi

Ask a question about the subject (type 'exit' to quit, 'save' to save the conversation):
What can you tell me about this topic?

Based on the information in my knowledge base, I can provide you with detailed information about [domain-specific response based on loaded knowledge files]...

Ask a question about the subject (type 'exit' to quit, 'save' to save the conversation):
exit
```

## Configuration

The application supports the following configuration options through user secrets:

### Required Settings

```powershell
dotnet user-secrets set "Azure:ModelName" "your-model-name"
dotnet user-secrets set "Azure:Endpoint" "your-endpoint"
```

### Optional Settings

```powershell
# Agent settings
dotnet user-secrets set "Agent:Name" "Your Agent Name"
dotnet user-secrets set "Agent:Domain" "your specialized domain name"
dotnet user-secrets set "Agent:ToneStyle" "scholarly but approachable"
dotnet user-secrets set "Agent:Temperature" "0.1"
dotnet user-secrets set "Agent:TopP" "0.1"
dotnet user-secrets set "Agent:MaxCompletionTokens" "4096"
dotnet user-secrets set "Agent:MaxPromptTokens" "8192"

# UI settings
dotnet user-secrets set "UI:WelcomeMessage" "Your Custom Welcome Message"
dotnet user-secrets set "UI:PromptMessage" "Your custom prompt message"
```

## Files Structure

### Instructions Files

Each markdown file in the `instructions/` directory will be loaded as part of the agent's knowledge base. The filename will be used as a section title in the knowledge base. You can add multiple instruction files, and they will all be combined into the agent's knowledge base.

The repository includes two example instruction files:
- `instructions/jabberwocky.md` - Basic information about the Jabberwocky
- `instructions/advanced_analysis.md` - More detailed analysis of the Jabberwocky poem

### Prompt Template

The `prompts/prompt_template.md` is a special file that defines the agent's behavior. It uses placeholders like `{{DOMAIN_NAME}}` and `{{TONE_STYLE}}` that will be replaced with your configuration values.

This template allows you to create a specialized agent without modifying the code.

## Customizing Content

The application comes with seed content about the Jabberwocky by default. This content is stored as static files in the repository.

If you want to change the default content to your own domain:

1. Edit the files in the `prompts` directory to customize the agent's behavior
   - `prompt_template.md` - Template that defines how the agent behaves
   
2. Edit or add files in the `instructions` directory to provide domain-specific knowledge
   - `jabberwocky.md` - Basic information about the Jabberwocky
   - `advanced_analysis.md` - More detailed analysis of the Jabberwocky poem
   
3. You can also modify the default agent configuration in `up.ps1` to match your content:
   - Agent name
   - Domain name
   - Tone style
   - Welcome and prompt messages

This approach allows you to distribute a pre-configured agent with your own specialized knowledge domain.

You can also modify the default agent configuration settings in the user secrets section at the end of the script:
```powershell
# Set default agent configuration
dotnet user-secrets set "Agent:Name" "Your Expert Name" --project "$CSHARP_PROJECT_PATH"
dotnet user-secrets set "Agent:Domain" "your specialized domain" --project "$CSHARP_PROJECT_PATH"
# ... other settings
```

## Cleanup

When you're done with the application, you can remove the Azure resources to avoid incurring costs:

```powershell
.\infra\down.ps1
```

## Error Handling

If you encounter an error about Azure resources not being found, make sure you've run the provisioning script:

```powershell
.\infra\up.ps1
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
