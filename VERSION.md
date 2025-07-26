# CAD_API Version History / 版本歷史

## v7.0.0 (2025-07-26) - 目前版本 / Current Version

### New Features / 新功能
- ✅ Selection functionality / 選擇功能實現
  - SELECT ALL, TYPE, WINDOW, CROSSING, POINT, LAYER, CLEAR
- ✅ Modification commands support selection sets / 修改命令支援選擇集
  - MOVE, COPY, ROTATE, SCALE, DELETE for SELECTED objects
- ✅ Extended drawing commands / 擴展繪圖命令
  - CIRCLE, ARC, RECTANGLE, POLYLINE, TEXT, DIMENSION, HATCH
- ✅ Extended modification commands / 擴展修改命令
  - MIRROR, OFFSET, TRIM, EXTEND

### Changes from v1.0.0 / 相較 v1.0.0 的變化
- Added comprehensive selection system / 新增完整的選擇系統
- Added support for multiple drawing types / 新增多種圖形類型支援
- Enhanced modification commands with selection support / 增強修改命令支援選擇集
- Improved command parsing and error handling / 改進命令解析和錯誤處理

---

## v1.0.0 (2025-01-26) - 初始版本 / Initial Release

### Features / 功能特性
- ✅ AutoCAD .NET API integration / AutoCAD .NET API 整合
- ✅ FileWatcher monitoring mechanism / 文件監視機制
- ✅ DRAW LINE command support / 支援 DRAW LINE 命令
- ✅ PowerShell script automation / PowerShell 腳本自動化
- ✅ CLI control interface / CLI 控制介面

### Documentation / 文檔
- 📖 Bilingual README (English/Chinese) / 雙語 README
- 📖 Complete User Guide in both languages / 雙語完整使用指南
- 📖 LLM development guide / LLM 開發指南
- 📖 MIT License / MIT 許可證

### Known Limitations / 已知限制
- Only supports DRAW LINE command / 僅支援 DRAW LINE 命令
- Requires 1-second delay between commands / 命令間需要 1 秒延遲
- One-way communication only / 僅支援單向通信