# Jabberwocky AI Agent

This repository contains a custom AI agent built using Azure's Persistent Agent capabilities. The agent specializes in knowledge about the mythical creature "Jabberwocky" from Lewis Carroll's poem and can answer questions about its literary origins, physical description, cultural impact, and the poem itself.

## Project Structure

- `Program.cs` - The main C# application that creates and interacts with the AI agent
- `knowledge/` - Directory containing the agent's knowledge base
  - `instructions.md` - Information about the Jabberwocky that serves as the agent's knowledge
  - `prompt.md` - Instructions that define the agent's behavior and responses
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

### 3. Build and Run the Application

```powershell
dotnet build
dotnet run
```

## Sample Interaction

Here's what a typical interaction with the agent looks like:

```
Jabberwocky AI Agent
------------------
Successfully loaded instructions (3166 chars) and prompt (1370 chars) from knowledge folder
Creating agent...
Agent created with ID: agent_abc123xyz789
Creating conversation thread...
Thread created with ID: thread_456def789ghi

Ask a question about the Jabberwocky (type 'exit' to quit, 'save' to save the conversation):
What is the Jabberwocky?

The Jabberwocky is a legendary creature that originated in Lewis Carroll's novel "Through the Looking-Glass, and What Alice Found There" (1871), which was the sequel to "Alice's Adventures in Wonderland." 

It's described as a fearsome monster with "jaws that bite" and "claws that catch," and is said to have eyes of flame. In the famous illustration by John Tenniel that accompanied the original text, the Jabberwocky appears as a dragon-like creature with a long neck, scales, wings, and a vest. It stands upright like a human but has features of various reptilian and avian creatures combined.

Ask a question about the Jabberwocky (type 'exit' to quit, 'save' to save the conversation):
exit
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
