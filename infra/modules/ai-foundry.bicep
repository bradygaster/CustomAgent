// ai-foundry.bicep - Module for AI Foundry project and agent services

@description('The Azure region for deploying resources')
param location string

@description('Base name used for resource naming')
param baseName string

@description('Environment name (dev, test, prod)')
param environmentName string

@description('Suffix to ensure unique resource names')
param resourceNameSuffix string

@description('Tags to apply to all resources')
param tags object

var aiFoundryName = 'foundry-${baseName}-${resourceNameSuffix}'

// Azure AI Foundry Cognitive Services Account
resource aiFoundryCognitiveServices 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: aiFoundryName
  location: location
  tags: tags
  kind: 'OpenAI'
  sku: {
    name: 'S0'
  }
  properties: {
    customSubDomainName: aiFoundryName
    networkAcls: {
      defaultAction: 'Allow'
      virtualNetworkRules: []
      ipRules: []
    }
    publicNetworkAccess: 'Enabled'
  }
}

// AI Foundry Project
resource aiFoundryProject 'Microsoft.CognitiveServices/accounts/projects@2023-05-01' = {
  parent: aiFoundryCognitiveServices
  name: 'jabberwocky-project'
  properties: {
    storageAccountName: aiFoundryCognitiveServices.name
    isDefault: false
  }
}

// GPT-4 Model Deployment
resource gpt4ModelDeployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = {
  parent: aiFoundryCognitiveServices
  name: 'gpt-4'
  sku: {
    name: 'Standard'
    capacity: 1
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-4'
      version: '0613'
    }
  }
}

// GPT-3.5 Turbo Model Deployment (backup/alternative model)
resource gpt35ModelDeployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = {
  parent: aiFoundryCognitiveServices
  name: 'gpt-35-turbo'
  sku: {
    name: 'Standard'
    capacity: 1
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-35-turbo'
      version: '0613'
    }
  }
}

// Outputs
output projectName string = aiFoundryProject.name
output projectEndpoint string = aiFoundryCognitiveServices.properties.endpoint
output agentServiceConnection string = 'https://${aiFoundryCognitiveServices.properties.endpoint}/ai-foundry/${aiFoundryProject.name}'
output availableModels array = [
  gpt4ModelDeployment.name
  gpt35ModelDeployment.name
]
