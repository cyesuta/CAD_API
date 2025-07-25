# PowerShell 腳本畫三個正方形 - 0.5秒延遲版本

# 取得腳本所在目錄的父目錄
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$parentDir = Split-Path -Parent $scriptDir

# CLI 路徑
$cliPath = Join-Path $parentDir "src\CAD_API.CLI\bin\Debug\net6.0\CAD_API.CLI.exe"

# 定義每條命令
$commandList = @(
    "DRAW LINE 0,0 1000 HORIZONTAL",
    "DRAW LINE 1000,0 1000 VERTICAL",
    "DRAW LINE 0,1000 1000 HORIZONTAL",
    "DRAW LINE 0,0 1000 VERTICAL",
    "DRAW LINE 1500,0 500 HORIZONTAL",
    "DRAW LINE 2000,0 500 VERTICAL",
    "DRAW LINE 1500,500 500 HORIZONTAL",
    "DRAW LINE 1500,0 500 VERTICAL",
    "DRAW LINE 2500,0 300 HORIZONTAL",
    "DRAW LINE 2800,0 300 VERTICAL",
    "DRAW LINE 2800,300 -300 HORIZONTAL",
    "DRAW LINE 2500,300 -300 VERTICAL"
)

Write-Host "開始繪製三個正方形，每條線延遲0.5秒..." -ForegroundColor Green

# 逐條執行命令
$lineNum = 1
foreach ($cmd in $commandList) {
    Write-Host "[$lineNum/12] $cmd" -ForegroundColor Yellow
    
    # 寫入臨時文件
    $tempFile = Join-Path $parentDir "temp_commands.txt"
    "$cmd`nEXIT" | Out-File -FilePath $tempFile -Encoding ASCII
    
    # 執行命令
    Start-Process -FilePath $cliPath -RedirectStandardInput $tempFile -Wait -NoNewWindow
    
    # 清理
    Remove-Item $tempFile
    
    # 延遲0.5秒
    Start-Sleep -Milliseconds 500
    
    $lineNum++
}

Write-Host "Drawing completed - Three squares with 0.5s delay!" -ForegroundColor Green