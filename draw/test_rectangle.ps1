# Test script for DRAW RECTANGLE command
# Usage: powershell -ExecutionPolicy Bypass -File test_rectangle.ps1

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$cliPath = Join-Path $rootDir "src\CAD_API.CLI\bin\Release\net6.0\CAD_API.CLI.exe"

# Draw rectangles using different formats
$commands = @(
    "DRAW RECTANGLE 0,0 1000,500"        # Using two corner points
    "DRAW RECTANGLE 1500,0 800 600"      # Using point + width + height
    "DRAW RECTANGLE 0,1000 1200,1800"    # Another two corners
)

foreach ($cmd in $commands) {
    Write-Host "Executing: $cmd" -ForegroundColor Cyan
    echo $cmd | & $cliPath
    Start-Sleep -Seconds 1
}

Write-Host "Rectangle test completed!" -ForegroundColor Green