param(
    [Parameter(Mandatory=$true)]
    [string]$Endpoint,
    
    [Parameter(Mandatory=$true)]
    [string]$ModelName
)

Write-Host "Setting up secrets for the Jabberwocky Agent..." -ForegroundColor Cyan

# Initialize user secrets if not already done
dotnet user-secrets init

# Set the secrets
dotnet user-secrets set "ConnectionStrings:AiAgentService" $Endpoint
dotnet user-secrets set "Azure:ModelName" $ModelName

Write-Host "Secrets set successfully!" -ForegroundColor Green
Write-Host "You can now run the application: dotnet run" -ForegroundColor Yellow
