# Test script for DRAW POLYLINE command
# Usage: powershell -ExecutionPolicy Bypass -File test_polyline.ps1

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$cliPath = Join-Path $rootDir "src\CAD_API.CLI\bin\Release\net6.0\CAD_API.CLI.exe"

# Draw polylines
$commands = @(
    "DRAW POLYLINE 0,0 500,200 700,0 1000,300"                    # Open polyline
    "DRAW POLYLINE 1500,0 1800,0 1800,500 1500,500 CLOSED"       # Closed polyline (square)
    "DRAW POLYLINE 100,1000 300,1200 500,1000 700,1200 900,1000" # Zigzag pattern
)

foreach ($cmd in $commands) {
    Write-Host "Executing: $cmd" -ForegroundColor Cyan
    echo $cmd | & $cliPath
    Start-Sleep -Seconds 1
}

Write-Host "Polyline test completed!" -ForegroundColor Green