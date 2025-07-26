# Test script for DRAW CIRCLE command
# Usage: powershell -ExecutionPolicy Bypass -File test_circle.ps1

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$cliPath = Join-Path $rootDir "src\CAD_API.CLI\bin\Debug\net6.0\CAD_API.CLI.exe"

# Draw three circles
$commands = @(
    "DRAW CIRCLE 0,0 500"
    "DRAW CIRCLE 1500,0 300"
    "DRAW CIRCLE 750,1000 400"
)

foreach ($cmd in $commands) {
    Write-Host "Executing: $cmd" -ForegroundColor Cyan
    @($cmd, "EXIT") | & $cliPath | Out-Null
    Start-Sleep -Seconds 1
}

Write-Host "Circle test completed!" -ForegroundColor Green