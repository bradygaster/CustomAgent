param(
    [Parameter()]
    [string]$ResourceGroupName = "rg-jabberwocky-agent-dev",
    
    [Parameter()]
    [string]$Location = "eastus",
    
    [Parameter()]
    [string]$EnvironmentName = "dev",
    
    [Parameter()]
    [string]$BaseName = "jabberwocky"
)

$ErrorActionPreference = "Stop"

Write-Host "Starting deployment of Jabberwocky AI Agent infrastructure..." -ForegroundColor Cyan

# Check if Az module is installed
if (-not (Get-Module -ListAvailable -Name Az)) {
    Write-Error "The Az PowerShell module is not installed. Please run 'Install-Module -Name Az -AllowClobber -Scope CurrentUser' to install it."
    exit 1
}

# Check if user is logged in to Azure
try {
    $context = Get-AzContext
    if (-not $context) {
        Write-Host "Not logged in to Azure. Please login..." -ForegroundColor Yellow
        Connect-AzAccount
    }
    else {
        Write-Host "Already logged in as $($context.Account)" -ForegroundColor Green
    }
}
catch {
    Write-Host "Not logged in to Azure. Please login..." -ForegroundColor Yellow
    Connect-AzAccount
}

# Create or check resource group
try {
    $resourceGroup = Get-AzResourceGroup -Name $ResourceGroupName -ErrorAction SilentlyContinue
    if (-not $resourceGroup) {
        Write-Host "Creating resource group $ResourceGroupName in location $Location..." -ForegroundColor Yellow
        $resourceGroup = New-AzResourceGroup -Name $ResourceGroupName -Location $Location
    }
    else {
        Write-Host "Resource group $ResourceGroupName already exists." -ForegroundColor Green
    }
}
catch {
    Write-Error "Failed to create or check resource group: $_"
    exit 1
}

# Deploy Bicep template
try {
    Write-Host "Deploying infrastructure using Bicep templates..." -ForegroundColor Yellow
    
    $deploymentName = "jabberwocky-deployment-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
    $templateFile = Join-Path $PSScriptRoot "main.bicep"
    $parameterFile = Join-Path $PSScriptRoot "main.parameters.json"
    
    # Override parameters if provided
    $additionalParams = @{}
    if ($EnvironmentName) {
        $additionalParams["environmentName"] = $EnvironmentName
    }
    if ($BaseName) {
        $additionalParams["baseName"] = $BaseName
    }
    
    $deployment = New-AzResourceGroupDeployment `
        -Name $deploymentName `
        -ResourceGroupName $ResourceGroupName `
        -TemplateFile $templateFile `
        -TemplateParameterFile $parameterFile `
        @additionalParams `
        -Verbose
    
    # Display deployment outputs
    Write-Host "Deployment completed successfully!" -ForegroundColor Green
    Write-Host "AI Foundry Project Name: $($deployment.Outputs.aiFoundryProjectName.Value)" -ForegroundColor Cyan
    Write-Host "AI Foundry Project Endpoint: $($deployment.Outputs.aiFoundryProjectEndpoint.Value)" -ForegroundColor Cyan
    Write-Host "AI Agent Service Connection: $($deployment.Outputs.aiAgentServiceConnection.Value)" -ForegroundColor Cyan
    Write-Host "Available Models:" -ForegroundColor Cyan
    $deployment.Outputs.availableModels.Value | ForEach-Object {
        Write-Host "  - $_" -ForegroundColor Cyan
    }
    
    # Set up the secrets for the application
    Write-Host "`nDo you want to set up the user secrets for the application? (Y/N)" -ForegroundColor Yellow
    $setupSecrets = Read-Host
    
    if ($setupSecrets -eq "Y" -or $setupSecrets -eq "y") {
        $connectionString = $deployment.Outputs.aiAgentServiceConnection.Value
        $modelName = $deployment.Outputs.availableModels.Value[0] # Default to first model (GPT-4)
        
        Write-Host "Setting up secrets using: $connectionString and $modelName" -ForegroundColor Yellow
        
        # Navigate up one directory to the project root
        $projectRootPath = Split-Path -Parent $PSScriptRoot
        Set-Location $projectRootPath
        
        # Run the setup_secrets.ps1 script
        & "$projectRootPath\setup_secrets.ps1" -Endpoint $connectionString -ModelName $modelName
    }

    return $deployment.Outputs
}
catch {
    Write-Error "Deployment failed: $_"
    exit 1
}
