# Jabberwocky AI Agent

This simple console application creates an AI agent that specializes exclusively in information about the mythical creature known as the Jabberwocky. The agent is powered by Azure AI Agent Services and can answer questions related only to the Jabberwocky based on the provided knowledge base.

## Prerequisites

- .NET 9.0 SDK or later
- Azure account with appropriate permissions
- Azure CLI or Azure PowerShell (for infrastructure deployment)

## Infrastructure Deployment

The project includes infrastructure as code (IaC) to provision all required Azure resources:

1. Navigate to the `infra` directory:
   ```
   cd infra
   ```

2. Deploy the Azure resources using one of the following methods:

   **PowerShell**:
   ```powershell
   ./deploy.ps1
   ```

   **Bash**:
   ```bash
   ./deploy.sh
   ```

3. The deployment script will:
   - Create a resource group (if it doesn't exist)
   - Deploy Azure AI Foundry resources
   - Set up model deployments (GPT-4 and GPT-3.5 Turbo)
   - Offer to configure application secrets automatically

For more deployment options and customization, see the [infrastructure README](infra/README.md).

## Setup

1. Clone the repository and navigate to the CustomAgent directory.

2. Restore NuGet packages:
   ```
   dotnet restore
   ```

3. Configure the application settings using the provided setup script. You will need:
   - The Azure AI Agent Service endpoint URL
   - The model deployment name

   Run the setup script:
   ```powershell
   .\setup_secrets.ps1 -Endpoint "your-endpoint-url" -ModelName "your-model-name"
   ```

   Or manually set up the user secrets:
   ```
   dotnet user-secrets set "ConnectionStrings:AiAgentService" "your-endpoint-url"
   dotnet user-secrets set "Azure:ModelName" "your-model-name"
   ```

## Running the Application

Simply run the application using:
```
dotnet run
```

## Usage

Once the application is running:
1. Enter your questions about the Jabberwocky when prompted.
2. The agent will respond with information from its knowledge base.
3. Type 'exit' to quit and delete the agent and thread.
4. Type 'save' to quit but preserve the conversation in Azure AI Foundry (accessible at https://ai.azure.com).

## Customization

You can customize the agent by modifying the following files:
- `instructions.md`: Contains the knowledge base about the Jabberwocky.
- `prompt.md`: Contains the instructions for the agent on how to respond to queries.

## Architecture

The application:
1. Creates an Azure AI Agent using the provided instructions and knowledge base.
2. Creates a thread for the conversation.
3. Processes user inputs and sends them to the agent.
4. Streams the agent's responses back to the console.
5. Optionally saves the conversation for later review or cleans up resources on exit.
