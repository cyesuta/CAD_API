# CAD_API 專案建構指南 - 給 LLM 的完整 Prompt

如果你想使用 Claude Code 或其他 LLM 輔助生成類似的 AutoCAD 控制工具，以下是完整的 prompt：

---

## 完整 Prompt

我需要你幫我建立一個 CAD_API 工具，用來通過程式控制 AutoCAD 進行繪圖操作。請注意以下關鍵要求和實施細節：

### 核心需求
1. **必須使用 AutoCAD .NET API**，不要使用 COM 方式
2. 目標環境是 **AutoCAD 2023**，使用 **.NET Framework 4.8**
3. 需要支援從外部程式發送命令到 AutoCAD 執行
4. 實現 DRAW LINE 命令，格式：`DRAW LINE 起點X,起點Y 長度 方向`
5. 方向支援 HORIZONTAL 和 VERTICAL，長度支援負值（反向繪製）

### 專案架構
請建立以下結構：
```
CAD_API/
├── src/
│   ├── CAD_API.Plugin/      # AutoCAD 插件 (.NET Framework 4.8)
│   ├── CAD_API.CLI/         # CLI 客戶端 (.NET 6.0)
│   └── CAD_API.sln          # Visual Studio 解決方案
├── draw/                     # PowerShell 繪圖腳本
├── commands.txt             # 命令文件（FileWatcher 監視）
└── 文檔檔案
```

### 技術實現要點

#### 1. AutoCAD 插件部分 (CAD_API.Plugin)
創建一個 AutoCAD .NET 插件，包含：

**CADCommands.cs** - 主要命令類：
- 使用 `[CommandMethod("CADAPI_WATCH")]` 註冊命令
- 實現 FileWatcher 監視 commands.txt 文件變化
- 當文件變化時，讀取內容並執行命令
- 實現 `ParseAndExecuteCommand` 方法解析 DRAW LINE 命令
- 使用 Transaction 進行 AutoCAD 繪圖操作

**FileWatcher.cs** - 文件監視器：
- 使用 `FileSystemWatcher` 監視 commands.txt
- 設定 `NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size`
- 文件變化後延遲 100ms 再讀取（確保寫入完成）
- 讀取後立即清空文件內容
- 使用 `SendStringToExecute` 在 AutoCAD 主線程執行命令

**重要細節**：
- DLL 名稱使用版本號（如 CAD_API.Plugin.v3.dll）避免鎖定問題
- 引用 AcCoreMgd.dll、AcDbMgd.dll、AcMgd.dll
- 設定引用的 Copy Local = False

#### 2. CLI 客戶端部分 (CAD_API.CLI)
**SimpleClient.cs**：
```csharp
public void SendCommand(string command)
{
    File.WriteAllText(_commandFile, command);
    Thread.Sleep(500);
}
```
- 使用 `File.WriteAllText` 寫入（重要！不能用其他方式）
- 寫入後等待 500ms

#### 3. PowerShell 腳本部分
創建執行腳本時要注意：
- **每個命令必須間隔至少 1 秒**（這是關鍵！）
- 使用 CLI.exe 寫入命令，不要直接寫文件
- 範例腳本結構：
```powershell
$cliPath = "$PSScriptRoot\..\src\CAD_API.CLI\bin\Debug\net6.0\CAD_API.CLI.exe"
$commands = @(
    "DRAW LINE 0,0 1000 HORIZONTAL",
    "DRAW LINE 1000,0 1000 VERTICAL"
)
foreach ($cmd in $commands) {
    $tempFile = [System.IO.Path]::GetTempFileName()
    "$cmd`nEXIT" | Out-File -FilePath $tempFile -Encoding ASCII
    Start-Process -FilePath $cliPath -RedirectStandardInput $tempFile -Wait -NoNewWindow
    Remove-Item $tempFile
    Start-Sleep -Seconds 1  # 關鍵延遲！
}
```

### 命令解析邏輯
DRAW LINE 命令解析：
```csharp
// 格式：DRAW LINE startX,startY length direction
// 範例：DRAW LINE 0,0 1000 HORIZONTAL
switch (direction.ToUpper()) {
    case "HORIZONTAL":
    case "H":
        endX = startX + length;  // 支援負值
        endY = startY;
        break;
    case "VERTICAL": 
    case "V":
        endX = startX;
        endY = startY + length;  // 支援負值
        break;
}
```

### 關鍵問題和解決方案

1. **FileWatcher 不觸發問題**
   - 必須使用 CLI.exe 的 File.WriteAllText
   - PowerShell Out-File 不會正確觸發 FileSystemWatcher
   
2. **命令執行不完整問題**
   - 命令間隔必須至少 1 秒
   - FileWatcher 內部延遲 + 文件操作需要時間
   
3. **DLL 鎖定問題**
   - 使用版本號命名 DLL（v2, v3, v4）
   - AutoCAD 載入後無法覆蓋，需要新版本號

### 重要失敗經驗 - 絕對不要這樣做！

#### ❌ 錯誤方法 1：使用 COM 接口
```csharp
// 不要這樣做！
Type acType = Type.GetTypeFromProgID("AutoCAD.Application");
dynamic acApp = Activator.CreateInstance(acType);
```
**為什麼失敗**：
- 用戶明確要求使用 .NET API
- COM 方式效能較差且不穩定
- 需要處理複雜的 COM 釋放問題

#### ❌ 錯誤方法 2：使用命名管道通信
```csharp
// 不要這樣做！
var server = new NamedPipeServerStream("AutoCADPipe");
```
**為什麼失敗**：
- AutoCAD 插件環境下命名管道容易出現權限問題
- 跨進程通信複雜度高
- 調試困難

#### ❌ 錯誤方法 3：直接用 PowerShell 寫文件
```powershell
# 不要這樣做！
"DRAW LINE 0,0 1000 HORIZONTAL" | Out-File commands.txt
Set-Content commands.txt "DRAW LINE 0,0 1000 HORIZONTAL"
```
**為什麼失敗**：
- Out-File 和 Set-Content 不會觸發 FileSystemWatcher
- 文件寫入方式不產生正確的系統事件

#### ❌ 錯誤方法 4：批量發送命令
```powershell
# 不要這樣做！
$allCommands = $commands -join "`n"
$allCommands | Out-File $tempFile
```
**為什麼失敗**：
- FileWatcher 處理速度有限
- 批量命令會相互覆蓋
- 導致繪圖不完整（如正方形缺邊）

#### ❌ 錯誤方法 5：過短的延遲時間
```powershell
# 不要這樣做！
Start-Sleep -Milliseconds 500  # 太短！
```
**為什麼失敗**：
- 0.5 秒延遲會導致命令遺失
- FileWatcher 需要約 400-500ms 完成一個循環
- 必須使用 1 秒延遲確保穩定

#### ❌ 錯誤方法 6：忘記 assembly 級別的 CommandClass 屬性
```csharp
// 不要忘記在檔案頂部加入這行！
// [assembly: CommandClass(typeof(CAD_API.Plugin.CADCommands))]

namespace CAD_API.Plugin
{
    public class CADCommands
    {
        [CommandMethod("CADAPI_START")]
        public void StartServer() { }
    }
}
```
**為什麼失敗**：
- 缺少 assembly 級別的 CommandClass 屬性註冊
- 即使有 CommandMethod 屬性，命令也不會在 AutoCAD 中出現
- 這是 AutoCAD .NET API 的特殊要求

#### ❌ 錯誤方法 7：錯誤的坐標計算
```csharp
// 不要這樣做！
// 以為是終點坐標
endX = length;  // 錯誤！
endY = length;  // 錯誤！
```
**為什麼失敗**：
- 命令格式是「長度+方向」，不是終點坐標
- 必須根據起點和方向計算終點

### 正確做法總結

✅ **使用 .NET API + FileWatcher**
✅ **通過 CLI.exe 使用 File.WriteAllText**
✅ **每個命令間隔 1 秒**
✅ **使用版本號管理 DLL**
✅ **單條命令逐一執行**
✅ **正確計算終點坐標**

### 測試步驟
1. 在 AutoCAD 中：`NETLOAD` 載入插件
2. 執行：`CADAPI_WATCH` 啟動監視器
3. 運行 PowerShell 腳本繪製圖形
4. 驗證是否繪製出完整的圖形

### 預期結果
- 能夠從外部程式控制 AutoCAD 繪圖
- 支援繪製直線（水平、垂直、反向）
- 命令執行穩定可靠
- 支援批量繪圖腳本

### 重要提醒
1. 不要使用 COM 方式，必須用 .NET API
2. 不要使用命名管道或 Socket，使用文件監視
3. 命令間隔 1 秒是必須的，不是可選的
4. 使用 Transaction 確保 AutoCAD 操作的原子性

---

## 使用此 Prompt 的建議

1. **分階段實施**：先實現基本插件加載，再加入 FileWatcher，最後完善命令解析
2. **持續測試**：每個功能完成後立即在 AutoCAD 中測試
3. **保留版本**：使用版本號避免 DLL 鎖定問題
4. **文檔記錄**：記錄每個嘗試和解決方案，方便回溯

## 額外功能擴展建議

如果要擴展功能，可以考慮：
1. 支援更多圖形類型（CIRCLE, ARC, TEXT）
2. 支援顏色和圖層設定
3. 實現相對坐標系統
4. 添加錯誤處理和日誌
5. 支援批量命令執行優化

## 成功標準

當你能夠：
1. 從外部程式發送命令到 AutoCAD
2. 繪製出完整的圖形（如正方形）
3. 命令執行穩定不遺漏
4. 支援複雜的繪圖腳本

即表示工具開發成功！