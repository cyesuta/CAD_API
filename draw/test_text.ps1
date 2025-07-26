# Test script for DRAW TEXT command
# Usage: powershell -ExecutionPolicy Bypass -File test_text.ps1

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$cliPath = Join-Path $rootDir "src\CAD_API.CLI\bin\Release\net6.0\CAD_API.CLI.exe"

# Draw text with different sizes
$commands = @(
    'DRAW TEXT 0,0 "Hello AutoCAD" 200'
    'DRAW TEXT 0,500 "CAD_API v5" 150'
    'DRAW TEXT 0,900 "Drawing Test" 100'
    'DRAW TEXT 1500,0 "Large Text" 300'
)

foreach ($cmd in $commands) {
    Write-Host "Executing: $cmd" -ForegroundColor Cyan
    echo $cmd | & $cliPath
    Start-Sleep -Seconds 1
}

Write-Host "Text test completed!" -ForegroundColor Green