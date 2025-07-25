# PowerShell 腳本畫兩個正方形 - v3版本
# 大正方形已經正確，小正方形需要修正

$commands = @"
DRAW LINE 0,0 1000 HORIZONTAL
DRAW LINE 1000,0 1000 VERTICAL
DRAW LINE 0,1000 1000 HORIZONTAL
DRAW LINE 0,0 1000 VERTICAL
DRAW LINE 1500,0 500 HORIZONTAL
DRAW LINE 2000,0 500 VERTICAL
DRAW LINE 1500,500 500 HORIZONTAL
DRAW LINE 1500,0 500 VERTICAL
EXIT
"@

# 取得腳本所在目錄的父目錄
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$parentDir = Split-Path -Parent $scriptDir

# 寫入臨時文件
$tempFile = Join-Path $parentDir "temp_commands.txt"
$commands | Out-File -FilePath $tempFile -Encoding ASCII

# 執行 CLI
$cliPath = Join-Path $parentDir "src\CAD_API.CLI\bin\Debug\net6.0\CAD_API.CLI.exe"

Start-Process -FilePath $cliPath -RedirectStandardInput $tempFile -Wait -NoNewWindow

# 清理
Remove-Item $tempFile

Write-Host "Drawing completed!"