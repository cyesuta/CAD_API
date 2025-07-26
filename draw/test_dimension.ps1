# Test script for DRAW DIMENSION command
# Usage: powershell -ExecutionPolicy Bypass -File test_dimension.ps1

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$cliPath = Join-Path $rootDir "src\CAD_API.CLI\bin\Release\net6.0\CAD_API.CLI.exe"

# Draw dimensions
$commands = @(
    "DRAW DIMENSION 0,0 1000,0 200"        # Horizontal dimension with offset
    "DRAW DIMENSION 0,0 0,1000 200"        # Vertical dimension with offset
    "DRAW DIMENSION 1500,0 2000,500 300"   # Diagonal dimension
)

foreach ($cmd in $commands) {
    Write-Host "Executing: $cmd" -ForegroundColor Cyan
    echo $cmd | & $cliPath
    Start-Sleep -Seconds 1
}

Write-Host "Dimension test completed!" -ForegroundColor Green