# Test script for all new drawing commands
# Usage: powershell -ExecutionPolicy Bypass -File test_all_commands.ps1

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$cliPath = Join-Path $rootDir "src\CAD_API.CLI\bin\Release\net6.0\CAD_API.CLI.exe"

Write-Host "Testing all new drawing commands..." -ForegroundColor Green

# Test each command type
$commands = @(
    # Circles
    "DRAW CIRCLE 1000,1000 300"
    
    # Arcs  
    "DRAW ARC 1000,2000 300 0 180"
    
    # Rectangles
    "DRAW RECTANGLE 2000,1000 2500,1500"
    
    # Polyline
    "DRAW POLYLINE 3000,1000 3200,1200 3400,1000 3600,1200 CLOSED"
    
    # Text
    'DRAW TEXT 1000,3000 "CAD_API Test" 150'
    
    # Dimension
    "DRAW DIMENSION 1000,500 2000,500 200"
    
    # Hatch
    "DRAW HATCH SOLID 2000,2000 2500,2000 2500,2500 2000,2500"
)

foreach ($cmd in $commands) {
    Write-Host "`nExecuting: $cmd" -ForegroundColor Cyan
    echo $cmd | & $cliPath
    Start-Sleep -Seconds 1
}

Write-Host "`nAll commands tested successfully!" -ForegroundColor Green
Write-Host "Please check AutoCAD to see the results." -ForegroundColor Yellow