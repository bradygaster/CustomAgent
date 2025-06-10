// main.bicep - Root deployment template for Jabberwocky AI Agent resources

@description('The Azure region for deploying resources')
param location string = resourceGroup().location

@description('Environment name used for resource naming (e.g. dev, test, prod)')
@allowed([
  'dev'
  'test'
  'prod'
])
param environmentName string = 'dev'

@description('Base name used for resource naming')
param baseName string = 'jabberwocky'

@description('Tags to apply to all resources')
param tags object = {
  application: 'JabberwockyAgent'
  environment: environmentName
}

// Generate unique name suffix based on resource group ID
var uniqueSuffix = substring(uniqueString(resourceGroup().id), 0, 6)
var resourceNameSuffix = '${environmentName}-${uniqueSuffix}'

// Import module files
module aiFoundryModule 'modules/ai-foundry.bicep' = {
  name: 'aiFoundryDeployment'
  params: {
    location: location
    baseName: baseName
    environmentName: environmentName
    resourceNameSuffix: resourceNameSuffix
    tags: tags
  }
}

// Outputs
output aiFoundryProjectName string = aiFoundryModule.outputs.projectName
output aiFoundryProjectEndpoint string = aiFoundryModule.outputs.projectEndpoint
output aiAgentServiceConnection string = aiFoundryModule.outputs.agentServiceConnection
output availableModels array = aiFoundryModule.outputs.availableModels
