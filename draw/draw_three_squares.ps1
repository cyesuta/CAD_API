# PowerShell 腳本畫三個正方形

# 創建命令列表
# 第一個正方形 (0,0) 到 (1000,1000) - 邊長 1000
# 第二個正方形 (1500,0) 到 (2000,500) - 邊長 500
# 第三個正方形 (2500,0) 到 (2800,300) - 邊長 300
$commands = @"
DRAW LINE 0,0 1000 HORIZONTAL
DRAW LINE 1000,0 1000 VERTICAL
DRAW LINE 0,1000 1000 HORIZONTAL
DRAW LINE 0,0 1000 VERTICAL
DRAW LINE 1500,0 500 HORIZONTAL
DRAW LINE 2000,0 500 VERTICAL
DRAW LINE 1500,500 500 HORIZONTAL
DRAW LINE 1500,0 500 VERTICAL
DRAW LINE 2500,0 300 HORIZONTAL
DRAW LINE 2800,0 300 VERTICAL
DRAW LINE 2800,300 -300 HORIZONTAL
DRAW LINE 2500,300 -300 VERTICAL
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

# 分批執行命令以避免過快
$batchCommands = @(
    "DRAW LINE 0,0 1000 HORIZONTAL`nDRAW LINE 1000,0 1000 VERTICAL`nDRAW LINE 0,1000 1000 HORIZONTAL`nDRAW LINE 0,0 1000 VERTICAL`nEXIT",
    "DRAW LINE 1500,0 500 HORIZONTAL`nDRAW LINE 2000,0 500 VERTICAL`nDRAW LINE 1500,500 500 HORIZONTAL`nDRAW LINE 1500,0 500 VERTICAL`nEXIT",
    "DRAW LINE 2500,0 300 HORIZONTAL`nDRAW LINE 2800,0 300 VERTICAL`nDRAW LINE 2800,300 -300 HORIZONTAL`nDRAW LINE 2500,300 -300 VERTICAL`nEXIT"
)

foreach ($batch in $batchCommands) {
    $batch | Out-File -FilePath $tempFile -Encoding ASCII
    Start-Process -FilePath $cliPath -RedirectStandardInput $tempFile -Wait -NoNewWindow
    Remove-Item $tempFile
    Start-Sleep -Milliseconds 300
}


Write-Host "Drawing completed - Three squares!"