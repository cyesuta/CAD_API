# Test script for selection commands
# Usage: powershell -ExecutionPolicy Bypass -File test_selection.ps1

param([string]$cmd)
$cliPath = Join-Path $PSScriptRoot "..\src\CAD_API.CLI\bin\Debug\net6.0\CAD_API.CLI.exe"

function Execute-Command {
    param([string]$command)
    Write-Host "`nExecuting: $command" -ForegroundColor Cyan
    @($command, "EXIT") | & $cliPath | Out-Null
}

Write-Host "Testing selection commands..." -ForegroundColor Green

# 1. Draw some objects to select
Execute-Command "DRAW CIRCLE 0,0 300"
Start-Sleep -Seconds 1

Execute-Command "DRAW CIRCLE 500,0 200"
Start-Sleep -Seconds 1

Execute-Command "DRAW LINE 1000,0 500 HORIZONTAL"
Start-Sleep -Seconds 1

Execute-Command "DRAW RECTANGLE 0,500 400,800"
Start-Sleep -Seconds 1

# 2. Test SELECT TYPE
Write-Host "`n--- Testing SELECT TYPE CIRCLE ---" -ForegroundColor Yellow
Execute-Command "SELECT TYPE CIRCLE"
Start-Sleep -Seconds 1

# 3. Test MOVE SELECTED
Write-Host "`n--- Testing MOVE SELECTED ---" -ForegroundColor Yellow
Execute-Command "MOVE SELECTED 0,1000"
Start-Sleep -Seconds 1

# 4. Test SELECT WINDOW
Write-Host "`n--- Testing SELECT WINDOW ---" -ForegroundColor Yellow
Execute-Command "SELECT WINDOW -100,-100 600,600"
Start-Sleep -Seconds 1

# 5. Test COPY SELECTED
Write-Host "`n--- Testing COPY SELECTED ---" -ForegroundColor Yellow
Execute-Command "COPY SELECTED 1500,0 2"
Start-Sleep -Seconds 1

# 6. Test SELECT ALL
Write-Host "`n--- Testing SELECT ALL ---" -ForegroundColor Yellow
Execute-Command "SELECT ALL"
Start-Sleep -Seconds 1

# 7. Test SCALE SELECTED
Write-Host "`n--- Testing SCALE SELECTED ---" -ForegroundColor Yellow
Execute-Command "SCALE SELECTED 0,0 0.5"

Write-Host "`nSelection test completed!" -ForegroundColor Green