# Test script for DRAW ARC command
# Usage: powershell -ExecutionPolicy Bypass -File test_arc.ps1

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$cliPath = Join-Path $rootDir "src\CAD_API.CLI\bin\Release\net6.0\CAD_API.CLI.exe"

# Draw three arcs
$commands = @(
    "DRAW ARC 0,0 500 0 90"      # Quarter circle from 0 to 90 degrees
    "DRAW ARC 1500,0 300 45 135" # Arc from 45 to 135 degrees
    "DRAW ARC 750,1000 400 180 360" # Half circle from 180 to 360 degrees
)

foreach ($cmd in $commands) {
    Write-Host "Executing: $cmd" -ForegroundColor Cyan
    echo $cmd | & $cliPath
    Start-Sleep -Seconds 1
}

Write-Host "Arc test completed!" -ForegroundColor Green