using System;
using System.IO;
using System.Threading;

namespace CAD_API.CLI
{
    /// <summary>
    /// 簡單的文件通信客戶端
    /// </summary>
    public class SimpleClient
    {
        private readonly string _commandFile;

        public SimpleClient()
        {
            // 獲取應用程式的基礎目錄，向上尋找到 CAD_API 根目錄
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            
            // 從 bin\Debug\net6.0 向上找到 CAD_API 根目錄
            DirectoryInfo dir = new DirectoryInfo(currentDir);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "commands.txt")) && dir.Name != "CAD_API")
            {
                dir = dir.Parent;
            }
            
            // 如果找到 CAD_API 目錄或 commands.txt，使用該目錄；否則使用當前目錄
            string rootDir = (dir != null && (dir.Name == "CAD_API" || File.Exists(Path.Combine(dir.FullName, "commands.txt")))) 
                ? dir.FullName 
                : currentDir;
                
            _commandFile = Path.Combine(rootDir, "commands.txt");
            
            // 確保目錄存在
            string directory = Path.GetDirectoryName(_commandFile);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            Console.WriteLine($"使用命令文件：{_commandFile}");
        }

        /// <summary>
        /// 發送命令到 AutoCAD
        /// </summary>
        public void SendCommand(string command)
        {
            try
            {
                // 寫入命令到文件
                File.WriteAllText(_commandFile, command);
                Console.WriteLine($"命令已發送：{command}");
                
                // 等待一下讓 AutoCAD 處理
                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"發送命令失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 檢查文件是否可訪問
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                File.WriteAllText(_commandFile, "");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}