# Test script for DRAW HATCH command
# Usage: powershell -ExecutionPolicy Bypass -File test_hatch.ps1

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$cliPath = Join-Path $rootDir "src\CAD_API.CLI\bin\Release\net6.0\CAD_API.CLI.exe"

# Draw hatches with different patterns
$commands = @(
    "DRAW HATCH SOLID 0,0 500,0 500,500 0,500"                # Solid fill square
    "DRAW HATCH ANSI31 1000,0 1500,0 1500,500 1000,500"      # ANSI31 pattern
    "DRAW HATCH DOTS 0,1000 300,800 600,1000 300,1200"       # Triangle with dots pattern
)

foreach ($cmd in $commands) {
    Write-Host "Executing: $cmd" -ForegroundColor Cyan
    echo $cmd | & $cliPath
    Start-Sleep -Seconds 1
}

Write-Host "Hatch test completed!" -ForegroundColor Green