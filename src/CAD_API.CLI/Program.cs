using System;

namespace CAD_API.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("====================================");
            Console.WriteLine("AutoCAD CLI 控制工具 - 文件監視版");
            Console.WriteLine("====================================");
            Console.WriteLine();
            
            var client = new SimpleClient();

            // 測試文件訪問
            Console.WriteLine("正在測試文件通信...");
            if (!client.TestConnection())
            {
                Console.WriteLine("無法訪問命令文件！");
                Console.WriteLine("按任意鍵退出...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("文件通信就緒！");
            Console.WriteLine();
            Console.WriteLine("請確保在 AutoCAD 中執行了 CADAPI_WATCH 命令");
            Console.WriteLine();
            Console.WriteLine("命令格式: DRAW LINE 起點X,起點Y 長度 [方向]");
            Console.WriteLine("範例: DRAW LINE 0,0 2000 HORIZONTAL");
            Console.WriteLine("輸入 'EXIT' 退出程序");
            Console.WriteLine();

            // 主循環
            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (input.ToUpper() == "EXIT")
                    break;

                // 發送命令到 AutoCAD
                client.SendCommand(input);
            }

            Console.WriteLine("程序已退出。");
        }
    }
}