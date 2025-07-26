# CAD_API 開發歷史日誌

## 2025-01-25

### 專案起源
用戶要求在 CAD_API 目錄下建立一個工具，用來連接並即時控制 AutoCAD 的編輯動作，需要讀取和寫入功能。要求使用 .NET 方式而非 COM 方式操作。

### 開發歷程

#### 第一階段：架構設計與參考研究
1. **初始架構設計**
   - 創建了 README.md 設計文檔（繁體中文）
   - 規劃了模組化架構：ConnectionManager、CommandEngine、DocumentManager 等
   - 設計了命令格式和 API 接口

2. **GitHub 參考專案研究**
   - 搜尋並分析了 AutoCADCodePack 和 Linq2Acad 專案
   - 下載並研究了這些專案的實現方式
   - 創建了參考分析文檔

#### 第二階段：原型開發（失敗嘗試）
1. **COM 方式實現（錯誤方向）**
   - 最初創建了使用 COM 的 AutoCADConnector.cs
   - 實現了 CommandParser.cs 和 CLI Program.cs
   - **失敗原因**：用戶明確要求使用 .NET API，不要 COM

#### 第三階段：.NET 插件開發（正確方向）
1. **插件架構實現**
   - 創建 CADCommands.cs 使用 AutoCAD .NET API
   - 實現了 CADAPI、CADLINE、CADAPI_BATCH 命令
   - 針對 AutoCAD 2023 版本進行配置
   - 創建了編譯腳本 build.ps1

2. **成功加載插件**
   - 用戶使用 NETLOAD 命令成功加載插件
   - CADAPI_BATCH 命令可以正常工作
   - 成功畫出了第一條 2000mm 的水平線

#### 第四階段：CLI 控制實現（多次嘗試）
1. **命名管道方式（部分失敗）**
   - 創建了 CommandServer.cs 實現命名管道服務器
   - 實現了 CADAPI_START 命令啟動服務器
   - **問題**：CADAPI_START 命令沒有出現在 AutoCAD 中
   - **原因**：缺少 CommandClass 屬性註冊

2. **文件監視方式（最終成功）**
   - 創建了 FileWatcher.cs 監視 commands.txt
   - 實現了 CADAPI_WATCH 和 CADAPI_STOPWATCH 命令
   - 創建了 SimpleClient.cs 用於寫入命令文件

3. **DLL 鎖定問題**
   - AutoCAD 加載 DLL 後無法覆蓋更新
   - **解決方案**：創建不同版本 (v2, v3, v4) 的程序集名稱
   - 最終使用 v3 版本穩定運行

#### 第五階段：命令執行問題排查
1. **直接文件寫入失敗**
   - PowerShell 直接寫入文件不觸發 FileSystemWatcher
   - **原因**：文件寫入方式不產生正確的文件系統事件

2. **CLI.exe 執行成功**
   - 發現通過 CLI.exe 寫入可以正常工作
   - **解決方案**：使用 PowerShell 腳本調用 CLI.exe 並重定向輸入

#### 第六階段：繪圖邏輯調試
1. **初始繪圖混亂**
   - 嘗試畫兩個正方形，結果線條雜亂
   - **原因**：對命令格式理解錯誤，終點計算邏輯有誤

2. **命令格式改進**
   - v4 版本嘗試支援終點坐標格式：`DRAW LINE 起點X,起點Y 終點X,終點Y`
   - 修改了 ParseAndExecuteCommand 方法支援兩種格式
   - **問題**：v4 版本加載後沒有反應

3. **回退到 v3 成功**
   - 使用 v3 版本的簡單格式
   - 逐步調試每條線的繪製
   - 最終成功畫出兩個完整的正方形

### 關鍵技術發現
1. **FileSystemWatcher 觸發條件**
   - 需要特定的文件寫入方式才能觸發
   - CLI.exe 的寫入方式符合要求
   - PowerShell 的 Out-File 或 Set-Content 不能正確觸發

2. **AutoCAD .NET API 使用**
   - 必須使用 Transaction 進行圖形數據庫操作
   - 需要正確引用 AcCoreMgd.dll、AcDbMgd.dll、AcMgd.dll
   - 命令必須使用 CommandMethod 屬性標記

3. **坐標系統理解**
   - HORIZONTAL 方向：X 坐標增加
   - VERTICAL 方向：Y 坐標增加
   - 負數長度可用於反向繪製

### 最終成功配置
- **插件版本**：v3
- **通信方式**：文件監視 (FileWatcher)
- **命令格式**：`DRAW LINE 起點X,起點Y 長度 方向`
- **執行方式**：PowerShell → CLI.exe → commands.txt → FileWatcher → AutoCAD

### 2025-01-25 更新

#### 新增功能
1. **繪圖腳本優化**
   - 創建 draw 子目錄集中管理繪圖腳本
   - 新增三個正方形繪製腳本（支援延遲執行）
   - 創建 draw_template.ps1 模板腳本

2. **文檔優化**
   - 在 TOOLS.md 新增詳細技術原理說明
   - 解釋 FileSystemWatcher 底層使用 Windows ReadDirectoryChangesW API
   - 加入系統呼叫堆疊分析
   - 說明為何需要 1 秒命令間隔

3. **目錄結構重組**
   - 將說明文檔移至 info 子目錄
   - 創建 docs/README.md 索引文件
   - 標記過時文檔

#### 技術發現
1. **FileWatcher 延遲問題**
   - 確認需要至少 1 秒間隔避免命令遺失
   - 內部延遲 100ms + 檔案操作 + AutoCAD 執行 ≈ 400-500ms
   
2. **PowerShell 寫入問題**
   - Out-File 不會正確觸發 FileSystemWatcher
   - 必須使用 CLI.exe 的 File.WriteAllText 方法

#### 已修復問題
1. 批次命令執行不完整 - 改用逐條執行模式
2. 命令解析支援負數長度

### 2025-01-25 文檔更新

#### 更新的文檔
1. **docs/CLI控制AutoCAD步驟.md** - 更新為 v3.dll 和 CADAPI_WATCH 方法
2. **docs/快速測試方法.md** - 更新為最新測試方法，標記為最新版本
3. **docs/快速開始.md** - 從 COM 方式更新為 .NET 插件方式
4. **docs/使用說明.md** - 更新為完整的 v3 版本使用說明
5. **docs/README.md** - 更新文檔狀態，標記所有文檔已更新

#### 文檔整理成果
- 所有過時的 COM 接口相關內容已移除
- 所有文檔統一更新為 v3 插件架構說明
- 加入「此文檔已更新為最新版本」標記
- 確保所有文檔反映 1 秒延遲要求

#### 新增 build.md
- 創建完整的 LLM prompt 指南
- 包含專案架構、技術細節、關鍵問題解決方案
- 提供給其他開發者使用 Claude Code 重現此工具的完整指引
- 新增「重要失敗經驗」章節，詳細列出 7 個常見錯誤方法及失敗原因

#### 文檔整合
- 將 4 個重複的使用說明文檔整合為單一「使用指南.md」
- 刪除了：快速測試方法.md、快速開始.md、使用說明.md、AutoCAD插件使用說明.md
- 新的使用指南包含完整的 12 個章節，涵蓋所有使用場景
- 更新 docs/README.md 反映文檔整合

#### 路徑通用化
- 修改所有文檔中的絕對路徑為 `{CAD_API根目錄}` 佔位符
- 更新 take_screenshot.ps1 使用相對路徑
- 刪除過時的 run_cli_with_commands.bat（已被 PowerShell 腳本取代）
- draw 目錄下的腳本已使用 $PSScriptRoot，無需修改

#### 源代碼路徑修改
- **SimpleClient.cs**：使用 AppDomain.CurrentDomain.BaseDirectory 動態尋找 CAD_API 根目錄
- **FileWatcher.cs**：使用 Assembly.GetExecutingAssembly().Location 動態尋找根目錄
- **AutoCADConnector_NET.cs**：動態尋找 src 目錄來創建插件源代碼
- 所有源代碼現在都使用相對路徑，可在任何位置運行

#### README.md 重寫
- 完全重寫 README.md 以反映實際實現
- 移除理想化的架構描述
- 加入 FileWatcher 機制說明
- 更新為實際的專案結構
- 加入快速開始指南和重要注意事項

#### 專案清理
- 刪除未使用的 CAD_API.Core 專案（COM 方式，錯誤方向）
- 更新 CAD_API.sln 移除 Core 專案，加入 Plugin 專案
- 刪除過時的 run_cli_with_commands.bat 腳本

#### CLI 文檔整合
- 將 CLI_README.md 和 CLI控制AutoCAD步驟.md 整合為「CLI使用說明.md」
- 移除理想化的 cadctl 工具描述，保留實際實現的說明
- 加入完整的使用步驟、命令格式、故障排除等內容

#### 最小使用單位說明
- 在 README.md 中標註最小使用單位（⭐ 標記）
- 創建「快速使用說明.txt」供最小版本使用
- 明確列出只需要 1MB 的執行檔即可使用

#### 文檔資料夾重命名
- 將 info 資料夾改名為 docs（更符合業界標準）
- 更新所有文檔中的相關路徑引用
- 更新 CHANGELOG.md 中的歷史記錄路徑

### 2025-01-25 文檔重複內容清理

#### 清理的重複內容
1. **系統架構圖** - 刪除使用指南中的重複，保留 README.md 中的原始版本
2. **支援的 AutoCAD 命令** - 刪除 README.md 和 TOOLS.md 中的重複，保留在使用指南中
3. **環境需求** - 刪除使用指南中的重複，保留 README.md 中的版本
4. **疑難排解** - 刪除 TOOLS.md 中的簡化版，保留使用指南中的詳細版本
5. **重要注意事項（1秒延遲）** - 刪除 README.md 和 TOOLS.md 中的重複說明，保留在使用指南中

#### 文檔職責劃分
- **README.md** - 專案概述、系統架構圖、環境需求等基本資訊
- **TOOLS.md** - 技術實現細節、原理深入分析
- **完整使用指南.md** - 詳細使用說明、命令清單、疑難排解等操作指引

### 2025-01-26 文檔更新與完善

#### 更新內容
1. **README.md 雙語化**
   - 英文版本在前，中文版本在後
   - 完整翻譯所有內容
   
2. **新增英文版使用指南**
   - docs/Complete_User_Guide.md
   - 與中文版完全對應
   
3. **build.md 英文化**
   - 移除中文內容，改為純英文
   - 更適合 LLM 使用
   
4. **新增 LICENSE 文件**
   - 雙語 MIT 許可證
   - 包含依賴項目說明

5. **新增貢獻指南**
   - .github/CONTRIBUTING.md
   - 雙語版本

6. **修正文檔路徑**
   - 統一 build.md 路徑指向 docs/build.md
   - 更新所有文檔引用

### 2025-01-26 功能更新 - 選擇功能實現

#### 新增選擇功能
1. **選擇命令實現** (SelectionCommands.cs)
   - SELECT ALL - 選擇所有物件
   - SELECT TYPE [類型] - 按類型選擇（支援 LINE, CIRCLE, ARC, POLYLINE, TEXT, DIMENSION, HATCH）
   - SELECT WINDOW 點1X,點1Y 點2X,點2Y - 窗口選擇
   - SELECT CROSSING 點1X,點1Y 點2X,點2Y - 交叉窗口選擇  
   - SELECT POINT X,Y [容差] - 點選擇（預設容差 10）
   - SELECT LAYER 圖層名 - 按圖層選擇
   - SELECT CLEAR - 清除選擇集

2. **修改命令支援選擇集**
   - MOVE SELECTED 偏移X,偏移Y - 移動選中的物件
   - COPY SELECTED 偏移X,偏移Y [複製數量] - 複製選中的物件
   - SCALE SELECTED 基點X,基點Y 比例因子 - 縮放選中的物件
   - ROTATE SELECTED 基點X,基點Y 角度 - 旋轉選中的物件
   - DELETE SELECTED - 刪除選中的物件

3. **測試腳本新增**
   - test_selection.ps1 - 完整測試選擇功能流程
   - 包含繪製測試圖形、各種選擇方式測試、修改操作測試

#### 技術實現細節
1. 使用靜態 ObjectIdCollection 儲存當前選擇集
2. 支援 LAST、SELECTED/SEL、ALL 作為目標物件指定
3. 所有選擇操作都在 Transaction 中進行
4. 選擇結果即時回饋給使用者

### 待解決問題
1. 截圖功能已實現但圖像質量可優化
2. 需要支援更多圖形類型（圓、弧、文字等）
3. 顏色和圖層管理功能待實現
4. 太陽系繪圖功能待完成