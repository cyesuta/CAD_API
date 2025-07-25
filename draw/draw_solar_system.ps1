# draw_solar_system.ps1
# 繪製太陽系示意圖

# 設置路徑
$scriptDir = $PSScriptRoot
$rootDir = Split-Path $scriptDir -Parent
$cliPath = Join-Path $rootDir "src\CAD_API.CLI\bin\Debug\net6.0\CAD_API.CLI.exe"

# 檢查 CLI.exe 是否存在
if (-not (Test-Path $cliPath)) {
    Write-Host "錯誤：找不到 CLI.exe 在：$cliPath" -ForegroundColor Red
    exit 1
}

Write-Host "開始繪製太陽系..." -ForegroundColor Green
Write-Host "使用 CLI: $cliPath"

# 定義太陽系參數
# 使用簡化的比例，單位為 AutoCAD 單位
$sunRadius = 5000  # 太陽半徑
$centerX = 0       # 中心點 X
$centerY = 0       # 中心點 Y

# 定義命令列表
$commands = @(
    # 繪製太陽（使用正方形近似圓形）
    "DRAW LINE -5000,-5000 10000 HORIZONTAL",  # 底邊
    "DRAW LINE 5000,-5000 10000 VERTICAL",     # 右邊
    "DRAW LINE 5000,5000 -10000 HORIZONTAL",   # 頂邊
    "DRAW LINE -5000,5000 -10000 VERTICAL",    # 左邊
    
    # 標記太陽中心
    "DRAW LINE -500,0 1000 HORIZONTAL",        # 水平十字線
    "DRAW LINE 0,-500 1000 VERTICAL"           # 垂直十字線
)

# 執行命令
foreach ($cmd in $commands) {
    Write-Host "執行命令: $cmd"
    
    # 使用管道將命令輸入到 CLI
    $cmd | & $cliPath
    
    # 必須延遲 1 秒，確保 FileWatcher 有足夠時間處理
    Start-Sleep -Seconds 1
}

Write-Host "`n太陽繪製完成！" -ForegroundColor Green
Write-Host "太陽位於原點 (0,0)，半徑 5000 單位"
Write-Host "下一步將繪製行星軌道和行星"