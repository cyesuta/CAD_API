using System;
using System.IO;
using System.IO.Pipes;

namespace CAD_API.CLI
{
    /// <summary>
    /// 命令客戶端 - 發送命令到 AutoCAD
    /// </summary>
    public class CommandClient
    {
        private const string PIPE_NAME = "CAD_API_PIPE";
        private NamedPipeClientStream _pipeClient;
        private StreamReader _reader;
        private StreamWriter _writer;
        private bool _isConnected;

        /// <summary>
        /// 連接到 AutoCAD 命令伺服器
        /// </summary>
        public bool Connect()
        {
            try
            {
                _pipeClient = new NamedPipeClientStream(".", PIPE_NAME, PipeDirection.InOut);
                _pipeClient.Connect(3000); // 3 秒超時
                
                _reader = new StreamReader(_pipeClient);
                _writer = new StreamWriter(_pipeClient) { AutoFlush = true };
                
                _isConnected = true;
                return true;
            }
            catch (TimeoutException)
            {
                Console.WriteLine("無法連接到 AutoCAD。請確保：");
                Console.WriteLine("1. AutoCAD 已啟動");
                Console.WriteLine("2. 插件已加載 (NETLOAD)");
                Console.WriteLine("3. 命令伺服器已啟動 (CADAPI_START)");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"連接錯誤：{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 發送命令並獲取結果
        /// </summary>
        public string SendCommand(string command)
        {
            if (!_isConnected)
            {
                return "ERROR: Not connected";
            }

            try
            {
                // 發送命令
                _writer.WriteLine(command);
                
                // 讀取結果
                string result = _reader.ReadLine();
                return result ?? "ERROR: No response";
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }

        /// <summary>
        /// 斷開連接
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _writer?.Close();
                _reader?.Close();
                _pipeClient?.Close();
                _isConnected = false;
            }
            catch { }
        }
    }
}