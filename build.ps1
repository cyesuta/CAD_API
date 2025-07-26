# build.ps1 - Build script for CAD_API.Plugin
param(
    [string]$Version = "v5"
)

$ErrorActionPreference = 'Stop'

# Get script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectPath = Join-Path $scriptDir "src\CAD_API.Plugin\CAD_API.Plugin.csproj"

# Check if project exists
if (-not (Test-Path $projectPath)) {
    Write-Error "Project file not found: $projectPath"
    exit 1
}

Write-Host "Building CAD_API.Plugin $Version..." -ForegroundColor Green

# Update assembly name in the project to include version
$csprojContent = Get-Content $projectPath -Raw
$newContent = $csprojContent -replace '<AssemblyName>CAD_API\.Plugin(\.v\d+)?</AssemblyName>', "<AssemblyName>CAD_API.Plugin.$Version</AssemblyName>"
Set-Content $projectPath $newContent

try {
    # Build the project
    dotnet build $projectPath -c Release
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nBuild succeeded!" -ForegroundColor Green
        Write-Host "Output: src\CAD_API.Plugin\bin\Release\net48\CAD_API.Plugin.$Version.dll" -ForegroundColor Cyan
    } else {
        Write-Error "Build failed with exit code $LASTEXITCODE"
    }
} finally {
    # Restore original assembly name
    Set-Content $projectPath $csprojContent
}