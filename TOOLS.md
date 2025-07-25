# CAD_API 工具文件說明

## 目錄結構
```
CAD_API/
├── src/                          # 源代碼目錄
│   ├── CAD_API.Plugin/          # AutoCAD 插件項目
│   ├── CAD_API.CLI/             # CLI 客戶端項目
│   ├── CAD_API.Core/            # 核心庫（已廢棄）
│   └── CAD_API.sln              # Visual Studio 解決方案
├── draw/                         # 繪圖腳本目錄
│   ├── draw_v3_squares.ps1      # 繪製兩個正方形
│   ├── execute_drawing.ps1      # 通用執行腳本
│   └── draw_template.ps1        # 繪圖腳本模板
├── take_screenshot.ps1          # 截圖腳本
├── commands.txt                 # 命令文件
├── autocad_screenshot.png       # 截圖輸出
└── 文檔檔案...
```

## 核心執行檔案

### 1. draw/draw_v3_squares.ps1 ✅ (主要使用)
**用途**：繪製兩個完整正方形的工作腳本
**使用方法**：
```bash
cd CAD_API
powershell -ExecutionPolicy Bypass -File draw\draw_v3_squares.ps1
```
**工作原理**：
- 生成繪圖命令
- 寫入臨時文件
- 調用 CLI.exe 執行
- 通過 FileWatcher 傳遞給 AutoCAD

### 2. draw/execute_drawing.ps1 ✅
**用途**：執行自定義繪圖命令的通用腳本
**使用方法**：
1. 編輯腳本中的 `$commands` 變量
2. 執行：`powershell -ExecutionPolicy Bypass -File draw\execute_drawing.ps1`

### 3. draw/draw_template.ps1 ✅
**用途**：繪圖腳本模板，可複製並修改
**使用方法**：
1. 複製此檔案並重命名
2. 修改 `$commands` 變量中的繪圖命令
3. 執行新腳本

### 4. take_screenshot.ps1 ✅
**用途**：截取 AutoCAD 視窗畫面
**使用方法**：
```bash
powershell -ExecutionPolicy Bypass -File take_screenshot.ps1
```
**輸出**：`autocad_screenshot.png`


## 插件與程序

### AutoCAD 插件 (v3)
**位置**：`src\CAD_API.Plugin\bin\Debug\net48\CAD_API.Plugin.v3.dll`
**載入方法**：
1. 在 AutoCAD 中輸入 `NETLOAD`
2. 選擇上述 DLL 檔案
3. 執行 `CADAPI_WATCH` 啟動文件監視

**支援的命令**：
請參考 [完整使用指南](docs/完整使用指南.md#autoCAD-命令清單) 查看所有支援的命令。

### CLI 客戶端
**位置**：`src\CAD_API.CLI\bin\Debug\net6.0\CAD_API.CLI.exe`
**用途**：寫入命令到 commands.txt
**注意**：不直接執行，通過 PowerShell 腳本調用


## 技術原理說明

### FileWatcher 監控機制

#### 1. 監控架構
```
CLI.exe → 寫入 commands.txt → FileSystemWatcher 偵測變化 → 讀取命令 → AutoCAD 執行
```

#### 2. 監控檔案
- **檔案路徑**：`{CAD_API根目錄}\commands.txt`
- **監控事件**：`NotifyFilters.LastWrite | NotifyFilters.Size`（檔案最後寫入時間或大小變化）

#### 3. 監控流程詳解

**步驟 1：FileWatcher 初始化**（FileWatcher.cs 第49-57行）
```csharp
_watcher = new FileSystemWatcher
{
    Path = directory,
    Filter = Path.GetFileName(CommandFile),
    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
};
_watcher.Changed += OnCommandFileChanged;
_watcher.EnableRaisingEvents = true;
```

**步驟 2：CLI 寫入命令**（SimpleClient.cs）
```csharp
public void SendCommand(string command)
{
    File.WriteAllText(_commandFile, command);
    Thread.Sleep(500);
}
```

**步驟 3：FileWatcher 偵測變化**（FileWatcher.cs 第94-132行）
- 檔案變化觸發 `OnCommandFileChanged` 事件
- 延遲 100ms 確保檔案寫入完成
- 最多重試 3 次讀取檔案內容
- 讀取命令後立即清空檔案

**步驟 4：命令執行**
- 透過 `SendStringToExecute` 在 AutoCAD 主執行緒執行
- 呼叫 `ParseAndExecuteCommand` 解析並執行繪圖命令

#### 4. 時序圖
```
時間軸 →
T0: CLI 寫入 "DRAW LINE 0,0 1000 HORIZONTAL"
T1: FileSystemWatcher 觸發 Changed 事件
T2: Thread.Sleep(100ms) 等待寫入完成
T3: 讀取 commands.txt 內容
T4: 清空 commands.txt
T5: AutoCAD 執行命令
T6: 繪製完成
```

#### 5. 為什麼需要 1 秒延遲？

**時間消耗分析**：
- FileWatcher 內部延遲：100ms
- 檔案讀取重試：最多 150ms（3次 × 50ms）
- 檔案清空操作：~50ms
- AutoCAD 命令執行：100-200ms
- **總計**：約 400-500ms

加上系統負載和其他因素，連續命令間隔小於 1 秒可能導致：
- 下一個命令在前一個處理完成前寫入
- FileWatcher 事件合併或遺失
- 命令被覆蓋或跳過

### CLI.exe 執行功能詳解

#### CLI 程式架構
```
Program.cs (主程式)
├── 初始化 SimpleClient
├── 測試檔案存取權限
├── 進入命令輸入循環
└── 接收使用者輸入 → SendCommand
```

#### SimpleClient 功能（SimpleClient.cs）
1. **檔案寫入功能**
   ```csharp
   File.WriteAllText(_commandFile, command);  // 使用 .NET 內建方法
   Thread.Sleep(500);  // 等待 AutoCAD 處理
   ```

2. **檔案系統操作**
   - 使用 `System.IO.File` 類別（.NET Framework 內建）
   - 使用 `System.IO.Directory` 類別建立目錄
   - 無需外部工具，純 .NET 實現

3. **執行流程**
   - 接收使用者輸入
   - 直接寫入 commands.txt
   - 等待 500ms
   - 返回等待下一個命令

### FileSystemWatcher 原理深入

#### 1. Windows API 基礎
FileSystemWatcher 底層使用 Windows API：
- `ReadDirectoryChangesW` - Windows 核心 API
- `I/O Completion Ports` - 非同步 I/O 機制
- 由 Windows 核心直接通知檔案變化

#### 2. 事件觸發機制
```
Windows 檔案系統
    ↓ (檔案變化)
Windows Kernel (ReadDirectoryChangesW)
    ↓ (核心通知)
.NET FileSystemWatcher
    ↓ (Changed 事件)
OnCommandFileChanged 處理函數
```

#### 3. 監控濾鏡詳解
```csharp
NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
```
- `LastWrite`：檔案最後寫入時間變更
- `Size`：檔案大小變更
- 使用位元 OR 運算結合多個濾鏡

#### 4. 為什麼 PowerShell 直接寫入會失敗？

**CLI.exe 寫入方式**：
```csharp
File.WriteAllText(path, content)
// 內部執行：
// 1. 開啟檔案 (CreateFile)
// 2. 寫入內容 (WriteFile)
// 3. 更新時間戳記
// 4. 關閉檔案 (CloseHandle)
// 5. 觸發 LastWrite 和 Size 變更
```

**PowerShell Out-File 問題**：
```powershell
$content | Out-File -FilePath $path
# 可能使用不同的寫入模式
# 可能不觸發預期的檔案系統事件
```

### 系統呼叫堆疊

```
使用者輸入 "DRAW LINE 0,0 1000 HORIZONTAL"
    ↓
CLI.exe (Console.ReadLine)
    ↓
SimpleClient.SendCommand
    ↓
File.WriteAllText (.NET API)
    ↓
Win32 CreateFile/WriteFile/CloseHandle
    ↓
NTFS 檔案系統更新
    ↓
ReadDirectoryChangesW 通知
    ↓
FileSystemWatcher.Changed 事件
    ↓
FileWatcher.OnCommandFileChanged
    ↓
AutoCAD SendStringToExecute
    ↓
CADCommands.ParseAndExecuteCommand
    ↓
AutoCAD .NET API (Transaction/Line/etc)
    ↓
AutoCAD 圖形引擎繪製
```

### 使用的技術棧

1. **CLI 端**
   - 純 .NET Framework 4.8 / .NET 6.0
   - System.IO 命名空間
   - 無外部相依性

2. **AutoCAD 插件端**
   - .NET Framework 4.8
   - AutoCAD .NET API
   - System.IO.FileSystemWatcher
   - 無需 COM Interop

3. **Windows 系統層**
   - ReadDirectoryChangesW API
   - NTFS 檔案系統通知
   - I/O Completion Ports

### 優勢與限制

**優勢**：
- 無需網路通訊（無防火牆問題）
- 無需 IPC 機制（命名管道、Socket 等）
- 簡單可靠，易於除錯
- 跨進程通訊穩定

**限制**：
- 檔案系統延遲（~100ms）
- 單一命令佇列（無並行）
- 需要檔案寫入權限
- 受限於檔案系統效能


## 疑難排解

請參考 [完整使用指南](docs/完整使用指南.md#疑難排解) 查看詳細的疑難排解方案。

## 注意事項

1. **版本管理**：目前使用 v3 版本，v4 有問題暫不使用
2. **命令文件**：commands.txt 會被自動清空
3. **坐標系統**：AutoCAD 使用標準笛卡爾坐標系
4. **文件路徑**：所有路徑使用絕對路徑
5. **重要：命令執行間隔**：請參考 [完整使用指南](docs/完整使用指南.md#為什麼需要-1-秒延遲) 了解詳細說明