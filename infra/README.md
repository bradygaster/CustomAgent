# Infrastructure Deployment for Jabberwocky AI Agent

This folder contains Bicep templates and deployment scripts for provisioning the necessary Azure resources for the Jabberwocky AI Agent application.

## Resources Created

The templates will provision the following resources:

- **Resource Group** (if it doesn't exist)
- **Azure AI Foundry (Cognitive Services) Account**
- **AI Foundry Project**
- **Model Deployments**:
  - GPT-4 model (primary)
  - GPT-3.5 Turbo model (backup/alternative)

## Prerequisites

- Azure subscription
- Azure CLI (for bash deployment) or Azure PowerShell (for PowerShell deployment)
- .NET SDK (for setting up user secrets)

## Deployment Options

### Using PowerShell

```powershell
# Default deployment
./deploy.ps1

# With custom parameters
./deploy.ps1 -ResourceGroupName "my-resource-group" -Location "westus" -EnvironmentName "test" -BaseName "custom-name"
```

### Using Bash

```bash
# Make the script executable
chmod +x deploy.sh

# Default deployment
./deploy.sh

# With custom parameters
./deploy.sh --resource-group "my-resource-group" --location "westus" --env "test" --base-name "custom-name"

# Get help
./deploy.sh --help
```

## Customization

You can customize the deployment by:

1. Modifying the `main.parameters.json` file
2. Providing parameters to the deployment script
3. Editing the Bicep templates directly for more advanced customization

## Post-Deployment

After deployment, both scripts offer to automatically configure the application secrets using the deployed resources. This will:

1. Initialize the user secrets store if needed
2. Set the AI Agent Service connection string
3. Set the default model name

## Manual Configuration

If you choose not to set up the secrets automatically or need to do it later, you can run:

```powershell
# From the project root directory
./setup_secrets.ps1 -Endpoint "<your-endpoint>" -ModelName "<your-model-name>"
```

Or manually set the secrets:

```
dotnet user-secrets set "ConnectionStrings:AiAgentService" "<your-endpoint>"
dotnet user-secrets set "Azure:ModelName" "<your-model-name>"
```

## Bicep Templates

- `main.bicep`: Entry point template that orchestrates the deployment
- `modules/ai-foundry.bicep`: Module for AI Foundry resources

## Notes

- The default region is set to East US, but you can specify a different region
- The default environment is "dev", options are "dev", "test", or "prod"
