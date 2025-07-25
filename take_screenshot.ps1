# PowerShell 截圖腳本
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# 等待2秒讓您切換到 AutoCAD
Write-Host "Please switch to AutoCAD window in 2 seconds..."
Start-Sleep -Seconds 2

# 截取整個屏幕
$bounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
$bitmap = New-Object System.Drawing.Bitmap $bounds.Width, $bounds.Height
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.CopyFromScreen($bounds.Location, [System.Drawing.Point]::Empty, $bounds.Size)

# 保存截圖
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$screenshotPath = Join-Path $scriptDir "autocad_screenshot.png"
$bitmap.Save($screenshotPath)

# 清理
$graphics.Dispose()
$bitmap.Dispose()

Write-Host "Screenshot saved to: $screenshotPath"