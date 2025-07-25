using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(CAD_API.Plugin.ServerCommands))]

namespace CAD_API.Plugin
{
    /// <summary>
    /// 命令伺服器 - 監聽來自 CLI 的命令
    /// </summary>
    public class CommandServer
    {
        private static CommandServer _instance;
        private static readonly object _lock = new object();
        private NamedPipeServerStream _pipeServer;
        private Thread _serverThread;
        private bool _isRunning;
        private CADCommands _commandProcessor;

        private CommandServer()
        {
            _commandProcessor = new CADCommands();
        }

        public static CommandServer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new CommandServer();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 啟動命令伺服器
        /// </summary>
        public void Start()
        {
            if (_isRunning)
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n命令伺服器已在運行中。");
                return;
            }

            _isRunning = true;
            _serverThread = new Thread(ServerLoop)
            {
                IsBackground = true,
                Name = "CAD_API_Server"
            };
            _serverThread.Start();

            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n命令伺服器已啟動，等待 CLI 連接...");
        }

        /// <summary>
        /// 停止命令伺服器
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            
            try
            {
                _pipeServer?.Close();
                _serverThread?.Join(1000);
            }
            catch { }

            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n命令伺服器已停止。");
        }

        /// <summary>
        /// 伺服器主循環
        /// </summary>
        private void ServerLoop()
        {
            while (_isRunning)
            {
                try
                {
                    // 創建命名管道
                    _pipeServer = new NamedPipeServerStream("CAD_API_PIPE", PipeDirection.InOut, 1, 
                        PipeTransmissionMode.Message, PipeOptions.Asynchronous);

                    // 等待客戶端連接
                    _pipeServer.WaitForConnection();

                    // 處理客戶端請求
                    using (StreamReader reader = new StreamReader(_pipeServer))
                    using (StreamWriter writer = new StreamWriter(_pipeServer) { AutoFlush = true })
                    {
                        while (_pipeServer.IsConnected && _isRunning)
                        {
                            string command = reader.ReadLine();
                            if (string.IsNullOrEmpty(command))
                                continue;

                            // 在主線程中執行命令
                            string result = ExecuteCommandInMainThread(command);
                            
                            // 發送結果回客戶端
                            writer.WriteLine(result);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    // 記錄錯誤但繼續運行
                    System.Diagnostics.Debug.WriteLine($"Server error: {ex.Message}");
                }
                finally
                {
                    _pipeServer?.Dispose();
                }

                // 短暫延遲後重新開始
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 在 AutoCAD 主線程中執行命令
        /// </summary>
        private string ExecuteCommandInMainThread(string command)
        {
            string result = "";

            // 使用 AutoCAD 的同步上下文在主線程執行
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute(
                "CADAPI_INTERNAL ", false, false, false);

            // 存儲命令供內部命令使用
            _pendingCommand = command;
            _commandCompleted = new ManualResetEventSlim(false);

            // 等待命令完成
            if (_commandCompleted.Wait(5000))
            {
                result = _commandResult;
            }
            else
            {
                result = "ERROR: Command timeout";
            }

            return result;
        }

        // 內部通信機制
        private static string _pendingCommand;
        private static string _commandResult;
        private static ManualResetEventSlim _commandCompleted;

        /// <summary>
        /// 內部命令處理器（在主線程中執行）
        /// </summary>
        [CommandMethod("CADAPI_INTERNAL", CommandFlags.NoHistory)]
        public static void ProcessInternalCommand()
        {
            if (string.IsNullOrEmpty(_pendingCommand))
                return;

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            try
            {
                // 使用現有的命令處理邏輯
                var commands = new CADCommands();
                var parseMethod = commands.GetType().GetMethod("ParseAndExecuteCommand", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                parseMethod?.Invoke(commands, new object[] { _pendingCommand, doc, db, ed });
                
                _commandResult = "SUCCESS: Command executed";
            }
            catch (System.Exception ex)
            {
                _commandResult = $"ERROR: {ex.Message}";
            }
            finally
            {
                _pendingCommand = null;
                _commandCompleted?.Set();
            }
        }
    }

    /// <summary>
    /// 命令伺服器控制命令
    /// </summary>
    public class ServerCommands
    {
        [CommandMethod("CADAPI_START")]
        public void StartServer()
        {
            CommandServer.Instance.Start();
        }

        [CommandMethod("CADAPI_STOP")]
        public void StopServer()
        {
            CommandServer.Instance.Stop();
        }

        [CommandMethod("CADAPI_STATUS")]
        public void ServerStatus()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("\n命令伺服器狀態：運行中");
            ed.WriteMessage("\n管道名稱：CAD_API_PIPE");
            ed.WriteMessage("\n使用 CADAPI_STOP 停止伺服器");
        }
    }
}