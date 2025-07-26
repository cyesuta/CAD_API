# CAD_API - AutoCAD .NET API Control Tool

## Project Overview

CAD_API is a control tool developed using the AutoCAD .NET API that enables external programs to control AutoCAD drawing operations through a file monitoring mechanism.

The motivation behind this project was to enable real-time control of AutoCAD using Claude Code. The COM-based approach proved unstable and occasionally failed to capture the AutoCAD process, so I switched to the .NET approach.

This project is still in its early stages. My goal is to create architectural floor plans using natural language (a dream). Updates are irregular, and new features are added as needed. Feel free to use it, and I'd be happy to receive feedback!

## Core Features

- ✅ Uses AutoCAD .NET API (not COM)
- ✅ File monitoring mechanism (FileWatcher)
- ✅ Supports batch drawing scripts
- ✅ Command execution interval control (1-second delay)
- ✅ Fully relative paths, can run from any location

## System Architecture

```
┌─────────────────────────────────────────────────────┐
│          PowerShell Scripts / User Input            │
└─────────────────────┬───────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────────┐
│               CAD_API.CLI.exe                       │
│         (Writes commands to commands.txt)           │
└─────────────────────┬───────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────────┐
│                commands.txt                         │
│         (FileWatcher monitors this file)            │
└─────────────────────┬───────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────────┐
│           CAD_API.Plugin.v3.dll                     │
│   (AutoCAD Plugin - FileWatcher + Command Parser)  │
└─────────────────────┬───────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────────┐
│                   AutoCAD                           │
│           (Executes drawing commands)               │
└─────────────────────────────────────────────────────┘
```

## Quick Start

### 1. Load Plugin
In AutoCAD:
```
NETLOAD
```
Select: `{CAD_API_Root}\src\CAD_API.Plugin\bin\Debug\net48\CAD_API.Plugin.v3.dll`

### 2. Start Monitor
```
CADAPI_WATCH
```

### 3. Execute Drawing
```bash
cd {CAD_API_Root}
powershell -ExecutionPolicy Bypass -File draw\draw_three_squares_delay.ps1
```

## Project Structure

```
CAD_API/
├── README.md                    # This file
├── CHANGELOG.md                 # Development history
├── TOOLS.md                     # Tool details
├── commands.txt                 # Command file (auto-generated)
│
├── src/                         # Source code
│   ├── CAD_API.Plugin/         # AutoCAD plugin (.NET Framework 4.8)
│   │   ├── CADCommands.cs      # Main command implementation
│   │   ├── FileWatcher.cs      # File watcher
│   │   └── bin/Debug/net48/
│   │       └── CAD_API.Plugin.v3.dll  # ⭐ Required: AutoCAD plugin
│   └── CAD_API.CLI/            # CLI client (.NET 6.0)
│       ├── Program.cs          # Main program
│       ├── SimpleClient.cs     # Command sender
│       └── bin/Debug/net6.0/
│           └── CAD_API.CLI.exe        # ⭐ Required: CLI executable
│
├── draw/                        # PowerShell drawing scripts
│   ├── draw_three_squares_delay.ps1  # ⭐ Required: Drawing script example
│   ├── draw_template.ps1            # Script template
│   └── README.md                    # Script documentation
│
└── docs/                        # Documentation
    ├── Complete_User_Guide.md   # Complete user guide (English)
    ├── 完整使用指南.md          # Complete user guide (Chinese)
    ├── build.md                 # LLM development guide
    └── README.md                # Documentation index
```

### Minimal Files Required (marked with ⭐)

If you only want to use the functionality without source code, you need at minimum:

```
CAD_API_Minimal/
├── CAD_API.Plugin.v3.dll       # ⭐ From src\CAD_API.Plugin\bin\Debug\net48\
├── CLI/                         # ⭐ From src\CAD_API.CLI\bin\Debug\net6.0\
│   ├── CAD_API.CLI.exe
│   ├── CAD_API.CLI.dll
│   ├── CAD_API.CLI.deps.json
│   └── CAD_API.CLI.runtimeconfig.json
├── draw_three_squares_delay.ps1 # ⭐ From draw\ folder
└── Quick_Start.txt
```

**Total Size**: ~1MB

**Requirements**:
- AutoCAD (tested on 2023, other versions welcome to test)
- .NET 6.0 Runtime ([Download](https://dotnet.microsoft.com/download/dotnet/6.0))
- Windows PowerShell (built-in)

## Supported AutoCAD Commands

See [Complete User Guide](docs/Complete_User_Guide.md#autocad-command-list) for all supported commands.

## Important Notes

See [Complete User Guide](docs/Complete_User_Guide.md#why-1-second-delay-is-needed) for detailed notes.

## System Requirements

- AutoCAD (tested on 2023, theoretically compatible with other versions)
- .NET Framework 4.8
- .NET 6.0 SDK (for CLI)
- Windows PowerShell

## Related Documentation

- **[TOOLS.md](TOOLS.md)** - Technical implementation details
- **[CHANGELOG.md](CHANGELOG.md)** - Development history
- **[build.md](docs/build.md)** - Guide for recreating this project with LLM
- **[docs/Complete_User_Guide.md](docs/Complete_User_Guide.md)** - Detailed usage guide

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

### Dependencies
- Requires AutoCAD (valid license required, tested on 2023)
- Requires .NET Framework 4.8 and .NET 6.0 Runtime
- Use of AutoCAD API is subject to Autodesk's terms and conditions

## Contributing and Support

- Report issues: [GitHub Issues](https://github.com/cyesuta/CAD_API/issues)
- Contributing: [Contributing Guide](.github/CONTRIBUTING.md)
- **LLM Development Guide**: [build.md](docs/build.md) - Complete prompts for recreating this tool with Claude Code

---

# CAD_API - AutoCAD .NET API 控制工具

## 專案概述

CAD_API 是一個使用 AutoCAD .NET API 開發的控制工具，透過文件監視機制實現從外部程式控制 AutoCAD 進行繪圖操作。

這個專案的緣由是我想使用 Claude Code 直接實時控制 AutoCAD，但因為 COM 的調用方式不夠穩定、偶爾抓不到 AutoCAD 進程。所以改用 .NET 的方式。

現在這個專案功能還非常初始，我的目標是可以用自然語言畫建築平面圖（夢想）。更新時間不定、更新功能不定。歡迎拿去用，如果能給我意見我會很開心。

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
│                   AutoCAD                           │
│            (執行繪圖命令)                            │
└─────────────────────────────────────────────────────┘
```

## 快速開始

### 1. 載入插件
在 AutoCAD 中：
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
    ├── Complete_User_Guide.md   # 完整使用指南（英文版）
    ├── 完整使用指南.md          # 完整使用指南（中文版）
    ├── build.md                 # LLM 開發指南
    └── README.md                # 文檔索引
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
- AutoCAD（已在 2023 測試，歡迎測試其他版本）
- .NET 6.0 Runtime（[下載連結](https://dotnet.microsoft.com/download/dotnet/6.0)）
- Windows PowerShell（內建）

## 支援的 AutoCAD 命令

請參考 [完整使用指南](docs/完整使用指南.md#autoCAD-命令清單) 查看所有支援的命令。

## 重要注意事項

請參考 [完整使用指南](docs/完整使用指南.md#為什麼需要-1-秒延遲) 了解詳細的注意事項。

## 環境需求

- AutoCAD（已在 2023 測試，理論上相容其他版本）
- .NET Framework 4.8
- .NET 6.0 SDK（用於 CLI）
- Windows PowerShell

## 相關文檔

- **[TOOLS.md](TOOLS.md)** - 工具技術詳解
- **[CHANGELOG.md](CHANGELOG.md)** - 開發歷史記錄
- **[build.md](docs/build.md)** - 使用 LLM 重現此專案的指南
- **[docs/完整使用指南.md](docs/完整使用指南.md)** - 詳細使用說明（中文版）
- **[docs/Complete_User_Guide.md](docs/Complete_User_Guide.md)** - Detailed usage guide (English)
- **[docs/Under_Construction.md](docs/Under_Construction.md)** - 開發路線圖與未來功能規劃

## 許可證

本專案採用 MIT 許可證 - 詳見 [LICENSE](LICENSE) 文件。

### 依賴項目
- 需要 AutoCAD（需要有效授權，已在 2023 測試）
- 需要 .NET Framework 4.8 和 .NET 6.0 Runtime
- AutoCAD API 的使用需遵守 Autodesk 的條款

## 貢獻與支援

- 問題回報：[GitHub Issues](https://github.com/cyesuta/CAD_API/issues)
- 貢獻指南：[Contributing Guide](.github/CONTRIBUTING.md)
- **LLM 開發指南**：[build.md](docs/build.md) - 使用 Claude Code 重現此工具的完整 prompt