# PowerShell 腳本執行繪圖命令

# 創建命令列表
$commands = @"
DRAW LINE 1000,1000 800 HORIZONTAL
DRAW LINE 1800,1000 800 VERTICAL
DRAW LINE 1800,1800 800 HORIZONTAL
DRAW LINE 1000,1800 800 VERTICAL
DRAW LINE 1400,1400 200 HORIZONTAL
DRAW LINE 1600,1400 200 VERTICAL
DRAW LINE 1600,1600 200 HORIZONTAL
DRAW LINE 1400,1600 200 VERTICAL
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