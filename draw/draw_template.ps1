# PowerShell 繪圖腳本模板
# 修改 $commands 變量來自定義繪圖命令
# 
# ⚠️ 重要：每個命令之間必須間隔至少 1 秒！
# 否則 FileWatcher 可能會漏掉某些命令

# 定義繪圖命令
$commands = @"
# 在這裡添加您的繪圖命令
# 格式: DRAW LINE 起點X,起點Y 長度 方向
# 範例:
# DRAW LINE 0,0 1000 HORIZONTAL
# DRAW LINE 1000,0 1000 VERTICAL
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

# ========== 選項 1：批次執行（可能會遺失命令）==========
# Start-Process -FilePath $cliPath -RedirectStandardInput $tempFile -Wait -NoNewWindow

# ========== 選項 2：逐行執行（推薦）==========
if (Test-Path $cliPath) {
    # 解析命令
    $commandLines = $commands -split "`n" | Where-Object { $_ -ne "EXIT" -and $_.Trim() -ne "" -and -not $_.StartsWith("#") }
    
    Write-Host "開始執行繪圖命令（每條命令延遲 1 秒）..." -ForegroundColor Green
    
    foreach ($cmd in $commandLines) {
        Write-Host "執行: $cmd" -ForegroundColor Yellow
        "$cmd`nEXIT" | Out-File -FilePath $tempFile -Encoding ASCII
        Start-Process -FilePath $cliPath -RedirectStandardInput $tempFile -Wait -NoNewWindow
        Remove-Item $tempFile
        Start-Sleep -Seconds 1
    }
    
    Write-Host "Drawing completed!" -ForegroundColor Green
} else {
    Write-Host "Error: CLI executable not found at $cliPath" -ForegroundColor Red
}