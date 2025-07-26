# Test script for modify commands
# Usage: powershell -ExecutionPolicy Bypass -File test_modify_commands.ps1

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$cliPath = Join-Path $rootDir "src\CAD_API.CLI\bin\Debug\net6.0\CAD_API.CLI.exe"

Write-Host "Testing modify commands..." -ForegroundColor Green

# Helper function to execute command
function Execute-Command {
    param([string]$cmd)
    Write-Host "`nExecuting: $cmd" -ForegroundColor Cyan
    @($cmd, "EXIT") | & $cliPath | Out-Null
}

# 1. First draw a rectangle to work with
Execute-Command "DRAW RECTANGLE 0,0 1000,500"
Start-Sleep -Seconds 1

# 2. Test COPY - make 3 copies
Execute-Command "COPY LAST 1200,0 3"
Start-Sleep -Seconds 1

# 3. Test MOVE - move the last object
Execute-Command "MOVE LAST 0,600"
Start-Sleep -Seconds 1

# 4. Draw a line to test ROTATE
Execute-Command "DRAW LINE 2000,2000 1000 HORIZONTAL"
Start-Sleep -Seconds 1

# 5. Test ROTATE - rotate 45 degrees
Execute-Command "ROTATE LAST 2000,2000 45"
Start-Sleep -Seconds 1

# 6. Draw another line to test SCALE
Execute-Command "DRAW LINE 0,3000 500 HORIZONTAL"
Start-Sleep -Seconds 1

# 7. Test SCALE - scale by 2x
Execute-Command "SCALE LAST 0,3000 2"
Start-Sleep -Seconds 1

# 8. Draw a line to test TRIM
Execute-Command "DRAW LINE 2000,3000 1000 HORIZONTAL"
Start-Sleep -Seconds 1

# 9. Test TRIM - trim to 600 units
Execute-Command "TRIM LAST 600"
Start-Sleep -Seconds 1

# 10. Draw a line to test EXTEND
Execute-Command "DRAW LINE 0,4000 500 HORIZONTAL"
Start-Sleep -Seconds 1

# 11. Test EXTEND - extend by 300 units
Execute-Command "EXTEND LAST 300"
Start-Sleep -Seconds 1

# 12. Draw a circle to test OFFSET
Execute-Command "DRAW CIRCLE 2000,4000 300"
Start-Sleep -Seconds 1

# 13. Test OFFSET - offset by 100 units
Execute-Command "OFFSET LAST 100"
Start-Sleep -Seconds 1

Write-Host "`nAll modify commands tested!" -ForegroundColor Green
Write-Host "Please check AutoCAD to see the results." -ForegroundColor Yellow