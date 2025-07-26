using System;
using System.IO;
using System.Threading;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(CAD_API.Plugin.FileWatcherCommands))]

namespace CAD_API.Plugin
{
    /// <summary>
    /// 文件監視命令 - 監視命令文件並執行
    /// </summary>
    public class FileWatcherCommands
    {
        private static FileSystemWatcher _watcher;
        private static string _commandFile;
        private static bool _isWatching = false;

        private static string CommandFile
        {
            get
            {
                if (_commandFile == null)
                {
                    // 獲取當前 DLL 的位置
                    string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(dllPath));
                    
                    // 從 bin\Debug\net48 向上找到 CAD_API 根目錄
                    while (dir != null && dir.Name != "CAD_API")
                    {
                        dir = dir.Parent;
                    }
                    
                    // 如果找到 CAD_API 目錄，使用該目錄；否則使用 DLL 所在目錄
                    string rootDir = (dir != null && dir.Name == "CAD_API") 
                        ? dir.FullName 
                        : Path.GetDirectoryName(dllPath);
                        
                    _commandFile = Path.Combine(rootDir, "commands.txt");
                }
                return _commandFile;
            }
        }

        [CommandMethod("CADAPI_WATCH")]
        public void StartWatching()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if (_isWatching)
            {
                ed.WriteMessage("\n文件監視器已在運行中。");
                return;
            }

            try
            {
                // 確保目錄存在
                string directory = Path.GetDirectoryName(CommandFile);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 創建空的命令文件
                if (!File.Exists(CommandFile))
                {
                    File.WriteAllText(CommandFile, "");
                }

                // 設置文件監視器
                _watcher = new FileSystemWatcher
                {
                    Path = directory,
                    Filter = Path.GetFileName(CommandFile),
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
                };

                _watcher.Changed += OnCommandFileChanged;
                _watcher.EnableRaisingEvents = true;
                _isWatching = true;

                ed.WriteMessage($"\n文件監視器已啟動。");
                ed.WriteMessage($"\n監視文件：{CommandFile}");
                ed.WriteMessage($"\n在該文件中寫入命令，AutoCAD 會自動執行。");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\n啟動文件監視器失敗：{ex.Message}");
            }
        }

        [CommandMethod("CADAPI_STOPWATCH")]
        public void StopWatching()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if (!_isWatching)
            {
                ed.WriteMessage("\n文件監視器未運行。");
                return;
            }

            try
            {
                _watcher?.Dispose();
                _watcher = null;
                _isWatching = false;
                ed.WriteMessage("\n文件監視器已停止。");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\n停止文件監視器失敗：{ex.Message}");
            }
        }

        private static void OnCommandFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                // 短暫延遲以確保文件寫入完成
                Thread.Sleep(100);

                // 讀取命令
                string command = "";
                int retries = 3;
                while (retries > 0)
                {
                    try
                    {
                        command = File.ReadAllText(CommandFile).Trim();
                        break;
                    }
                    catch
                    {
                        Thread.Sleep(50);
                        retries--;
                    }
                }

                if (string.IsNullOrEmpty(command))
                    return;

                // 清空文件以準備下一個命令
                File.WriteAllText(CommandFile, "");

                // 在主線程中執行命令
                Application.DocumentManager.MdiActiveDocument.SendStringToExecute(
                    "CADAPI_EXECFILE ", true, false, false);

                // 存儲命令供執行
                _pendingCommand = command;
            }
            catch { }
        }

        private static string _pendingCommand = "";

        [CommandMethod("CADAPI_EXECFILE", CommandFlags.NoHistory)]
        public void ExecuteFileCommand()
        {
            if (string.IsNullOrEmpty(_pendingCommand))
                return;

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            try
            {
                ed.WriteMessage($"\n執行命令：{_pendingCommand}");
                
                // 使用現有的命令處理邏輯
                var commands = new CADCommands();
                var parseMethod = commands.GetType().GetMethod("ParseAndExecuteExtendedCommand", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                parseMethod?.Invoke(commands, new object[] { _pendingCommand, doc, db, ed });
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\n命令執行失敗：{ex.Message}");
            }
            finally
            {
                _pendingCommand = "";
            }
        }
    }
}