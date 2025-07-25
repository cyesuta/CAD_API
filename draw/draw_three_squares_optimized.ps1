# PowerShell 腳本畫三個正方形 - 優化延遲版本

# 取得腳本所在目錄的父目錄
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$parentDir = Split-Path -Parent $scriptDir

# CLI 路徑
$cliPath = Join-Path $parentDir "src\CAD_API.CLI\bin\Debug\net6.0\CAD_API.CLI.exe"
$tempFile = Join-Path $parentDir "temp_commands.txt"

# 定義命令組（每組是一個正方形）
$squareGroups = @(
    @(
        "DRAW LINE 0,0 1000 HORIZONTAL",
        "DRAW LINE 1000,0 1000 VERTICAL", 
        "DRAW LINE 0,1000 1000 HORIZONTAL",
        "DRAW LINE 0,0 1000 VERTICAL"
    ),
    @(
        "DRAW LINE 1500,0 500 HORIZONTAL",
        "DRAW LINE 2000,0 500 VERTICAL",
        "DRAW LINE 1500,500 500 HORIZONTAL", 
        "DRAW LINE 1500,0 500 VERTICAL"
    ),
    @(
        "DRAW LINE 2500,0 300 HORIZONTAL",
        "DRAW LINE 2800,0 300 VERTICAL",
        "DRAW LINE 2800,300 -300 HORIZONTAL",
        "DRAW LINE 2500,300 -300 VERTICAL"
    )
)

Write-Host "開始繪製三個正方形..." -ForegroundColor Green

$squareNum = 1
foreach ($square in $squareGroups) {
    Write-Host "`n繪製第 $squareNum 個正方形:" -ForegroundColor Yellow
    
    # 將一個正方形的命令合併發送，命令之間加小延遲
    $commands = ""
    foreach ($cmd in $square) {
        $commands += "$cmd`n"
    }
    $commands += "EXIT"
    
    # 寫入並執行
    $commands | Out-File -FilePath $tempFile -Encoding ASCII
    Start-Process -FilePath $cliPath -RedirectStandardInput $tempFile -Wait -NoNewWindow
    
    # 清理
    Remove-Item $tempFile
    
    # 正方形之間延遲500毫秒
    if ($squareNum -lt 3) {
        Start-Sleep -Milliseconds 500
    }
    
    $squareNum++
}

Write-Host "`nDrawing completed - Three squares!" -ForegroundColor Green