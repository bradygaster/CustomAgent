#!/bin/bash

# Default parameters
RESOURCE_GROUP_NAME="rg-jabberwocky-agent-dev"
LOCATION="eastus"
ENVIRONMENT_NAME="dev"
BASE_NAME="jabberwocky"

# Process command line arguments
while [[ $# -gt 0 ]]; do
  key="$1"
  case $key in
    --resource-group|-g)
      RESOURCE_GROUP_NAME="$2"
      shift
      shift
      ;;
    --location|-l)
      LOCATION="$2"
      shift
      shift
      ;;
    --env|-e)
      ENVIRONMENT_NAME="$2"
      shift
      shift
      ;;
    --base-name|-n)
      BASE_NAME="$2"
      shift
      shift
      ;;
    --help|-h)
      echo "Usage: $0 [options]"
      echo "Options:"
      echo "  --resource-group, -g   Resource group name (default: rg-jabberwocky-agent-dev)"
      echo "  --location, -l         Azure region (default: eastus)"
      echo "  --env, -e              Environment name (dev, test, prod) (default: dev)"
      echo "  --base-name, -n        Base name for resources (default: jabberwocky)"
      echo "  --help, -h             Show this help message"
      exit 0
      ;;
    *)
      echo "Unknown option: $key"
      exit 1
      ;;
  esac
done

echo -e "\033[36mStarting deployment of Jabberwocky AI Agent infrastructure...\033[0m"

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    echo -e "\033[31mAzure CLI is not installed. Please install it: https://docs.microsoft.com/cli/azure/install-azure-cli\033[0m"
    exit 1
fi

# Check if user is logged in to Azure
if ! az account show &> /dev/null; then
    echo -e "\033[33mNot logged in to Azure. Please login...\033[0m"
    az login
else
    ACCOUNT=$(az account show --query user.name -o tsv)
    echo -e "\033[32mAlready logged in as $ACCOUNT\033[0m"
fi

# Create or check resource group
if ! az group show -n "$RESOURCE_GROUP_NAME" &> /dev/null; then
    echo -e "\033[33mCreating resource group $RESOURCE_GROUP_NAME in location $LOCATION...\033[0m"
    az group create --name "$RESOURCE_GROUP_NAME" --location "$LOCATION"
else
    echo -e "\033[32mResource group $RESOURCE_GROUP_NAME already exists.\033[0m"
fi

# Deploy Bicep template
echo -e "\033[33mDeploying infrastructure using Bicep templates...\033[0m"

DEPLOYMENT_NAME="jabberwocky-deployment-$(date +%Y%m%d-%H%M%S)"
TEMPLATE_FILE="$(dirname "$0")/main.bicep"
PARAMETER_FILE="$(dirname "$0")/main.parameters.json"

# Deploy the Bicep template
DEPLOYMENT_OUTPUT=$(az deployment group create \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --template-file "$TEMPLATE_FILE" \
    --parameters "$PARAMETER_FILE" \
    --parameters environmentName="$ENVIRONMENT_NAME" baseName="$BASE_NAME" \
    --output json)

if [ $? -ne 0 ]; then
    echo -e "\033[31mDeployment failed\033[0m"
    exit 1
fi

# Extract deployment outputs
AI_FOUNDRY_PROJECT_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.aiFoundryProjectName.value')
AI_FOUNDRY_PROJECT_ENDPOINT=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.aiFoundryProjectEndpoint.value')
AI_AGENT_SERVICE_CONNECTION=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.aiAgentServiceConnection.value')
AVAILABLE_MODELS=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.availableModels.value')
DEFAULT_MODEL=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.availableModels.value[0]')

# Display deployment outputs
echo -e "\033[32mDeployment completed successfully!\033[0m"
echo -e "\033[36mAI Foundry Project Name: $AI_FOUNDRY_PROJECT_NAME\033[0m"
echo -e "\033[36mAI Foundry Project Endpoint: $AI_FOUNDRY_PROJECT_ENDPOINT\033[0m"
echo -e "\033[36mAI Agent Service Connection: $AI_AGENT_SERVICE_CONNECTION\033[0m"
echo -e "\033[36mAvailable Models: $AVAILABLE_MODELS\033[0m"

# Set up the secrets for the application
echo -e "\033[33m\nDo you want to set up the user secrets for the application? (Y/N)\033[0m"
read SETUP_SECRETS

if [ "$SETUP_SECRETS" = "Y" ] || [ "$SETUP_SECRETS" = "y" ]; then
    # Navigate up one directory to the project root
    PROJECT_ROOT_PATH=$(dirname "$(dirname "$0")")
    cd "$PROJECT_ROOT_PATH"
    
    echo -e "\033[33mInitializing user secrets...\033[0m"
    dotnet user-secrets init
    
    echo -e "\033[33mSetting ConnectionStrings:AiAgentService to $AI_AGENT_SERVICE_CONNECTION\033[0m"
    dotnet user-secrets set "ConnectionStrings:AiAgentService" "$AI_AGENT_SERVICE_CONNECTION"
    
    echo -e "\033[33mSetting Azure:ModelName to $DEFAULT_MODEL\033[0m"
    dotnet user-secrets set "Azure:ModelName" "$DEFAULT_MODEL"
    
    echo -e "\033[32mSecrets set successfully!\033[0m"
    echo -e "\033[33mYou can now run the application: dotnet run\033[0m"
fi

echo -e "\033[32mDeployment and setup complete!\033[0m"
