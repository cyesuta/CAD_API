# 創建 Demo 版本倉庫

## 策略：創建一個簡化的公開版本

### 1. 創建新倉庫 `CAD_API_Demo`
- 只包含編譯後的 DLL
- 基本使用說明
- 不包含源代碼

### 2. Demo 倉庫結構
```
CAD_API_Demo/
├── README.md          # 簡化的使用說明
├── binaries/          # 編譯後的檔案
│   ├── CAD_API.Plugin.v3.dll
│   └── CLI/
│       └── CAD_API.CLI.exe
├── examples/          # 範例腳本
│   └── draw_three_squares.ps1
└── LICENSE            # MIT 許可證
```

### 3. Demo README.md 內容
```markdown
# CAD_API Demo Version

This is a demo version of CAD_API for testing purposes.

## Features
- Basic DRAW LINE command support
- File watcher functionality
- PowerShell script examples

## Full Version
For source code and complete documentation, please contact the author.

## License
MIT License - See LICENSE file
```

### 優點
- 朋友可以測試功能
- 不暴露源代碼
- 可以設為公開但風險較低