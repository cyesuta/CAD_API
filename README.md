# CAD_API - AutoCAD .NET API 控制工具

## 專案概述

CAD_API 是一個使用 AutoCAD .NET API 開發的控制工具，透過文件監視機制實現從外部程式控制 AutoCAD 進行繪圖操作。與傳統的 COM 方式不同，本工具採用 .NET 插件方式，提供更高效、更穩定的整合方案。

## 核心特性

- ✅ 使用 AutoCAD .NET API（非 COM）
- ✅ 文件監視機制（FileWatcher）
- ✅ 支援批量繪圖腳本
- ✅ 命令執行間隔控制（1秒延遲）
- ✅ 完全相對路徑，可在任何位置執行

## 系統架構

```
┌─────────────────────────────────────────────────────┐
│          PowerShell Scripts / User Input            │
└─────────────────────┬───────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────────┐
│               CAD_API.CLI.exe                       │
│         (寫入命令到 commands.txt)                    │
└─────────────────────┬───────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────────┐
│                commands.txt                         │
│            (FileWatcher 監視此檔案)                  │
└─────────────────────┬───────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────────┐
│           CAD_API.Plugin.v3.dll                     │
│    (AutoCAD 插件 - FileWatcher + 命令解析器)        │
└─────────────────────┬───────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────────┐
│              AutoCAD 2023                           │
│            (執行繪圖命令)                            │
└─────────────────────────────────────────────────────┘
```

## 快速開始

### 1. 載入插件
在 AutoCAD 2023 中：
```
NETLOAD
```
選擇：`{CAD_API根目錄}\src\CAD_API.Plugin\bin\Debug\net48\CAD_API.Plugin.v3.dll`

### 2. 啟動監視器
```
CADAPI_WATCH
```

### 3. 執行繪圖
```bash
cd {CAD_API根目錄}
powershell -ExecutionPolicy Bypass -File draw\draw_three_squares_delay.ps1
```

## 專案結構

```
CAD_API/
├── README.md                    # 本文件
├── CHANGELOG.md                 # 開發歷史
├── TOOLS.md                     # 工具詳細說明
├── build.md                     # LLM 開發指南
├── commands.txt                 # 命令檔案（自動生成）
│
├── src/                         # 源代碼
│   ├── CAD_API.Plugin/         # AutoCAD 插件 (.NET Framework 4.8)
│   │   ├── CADCommands.cs      # 主要命令實現
│   │   ├── FileWatcher.cs      # 文件監視器
│   │   └── bin/Debug/net48/
│   │       └── CAD_API.Plugin.v3.dll  # ⭐ 必需：AutoCAD 插件
│   └── CAD_API.CLI/            # CLI 客戶端 (.NET 6.0)
│       ├── Program.cs          # 主程式
│       ├── SimpleClient.cs     # 命令發送器
│       └── bin/Debug/net6.0/
│           └── CAD_API.CLI.exe        # ⭐ 必需：CLI 執行檔
│
├── draw/                        # PowerShell 繪圖腳本
│   ├── draw_three_squares_delay.ps1  # ⭐ 必需：繪圖腳本範例
│   ├── draw_template.ps1            # 腳本模板
│   └── README.md                    # 腳本說明
│
└── docs/                        # 文檔資料
    ├── 使用指南.md             # 完整使用指南（整合版）
    ├── CLI使用說明.md          # CLI 工具說明（整合版）
    └── README.md               # 文檔索引
```

### 最小使用單位（標註 ⭐ 的檔案）

如果只想使用功能而不需要源代碼，最少需要下載：

```
CAD_API_最小版本/
├── CAD_API.Plugin.v3.dll       # ⭐ 從 src\CAD_API.Plugin\bin\Debug\net48\
├── CLI/                         # ⭐ 從 src\CAD_API.CLI\bin\Debug\net6.0\
│   ├── CAD_API.CLI.exe
│   ├── CAD_API.CLI.dll
│   ├── CAD_API.CLI.deps.json
│   └── CAD_API.CLI.runtimeconfig.json
├── draw_three_squares_delay.ps1 # ⭐ 從 draw\ 資料夾
└── 快速使用說明.txt
```

**總大小**：約 1MB

**執行需求**：
- AutoCAD 2023
- .NET 6.0 Runtime（[下載連結](https://dotnet.microsoft.com/download/dotnet/6.0)）
- Windows PowerShell（內建）

## 支援的 AutoCAD 命令

請參考 [完整使用指南](docs/完整使用指南.md#autoCAD-命令清單) 查看所有支援的命令。

## 重要注意事項

請參考 [完整使用指南](docs/完整使用指南.md#為什麼需要-1-秒延遲) 了解詳細的注意事項。

## 環境需求

- AutoCAD 2023
- .NET Framework 4.8
- .NET 6.0 SDK（用於 CLI）
- Windows PowerShell

## 相關文檔

- **[TOOLS.md](TOOLS.md)** - 工具技術詳解
- **[CHANGELOG.md](CHANGELOG.md)** - 開發歷史記錄
- **[build.md](build.md)** - 使用 LLM 重現此專案的指南
- **[docs/使用指南.md](docs/使用指南.md)** - 詳細使用說明

## 許可證

本專案採用 MIT 許可證。

## 貢獻與支援

- 問題回報：[GitHub Issues](https://github.com/yourorg/CAD_API/issues)
- **LLM 開發指南**：[build.md](./build.md) - 使用 Claude Code 重現此工具的完整 prompt