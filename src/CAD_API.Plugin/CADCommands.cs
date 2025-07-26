using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Text.RegularExpressions;

[assembly: CommandClass(typeof(CAD_API.Plugin.CADCommands))]

namespace CAD_API.Plugin
{
    /// <summary>
    /// AutoCAD 插件命令類
    /// </summary>
    public partial class CADCommands
    {
        /// <summary>
        /// 主命令 - 接收格式化的繪圖命令
        /// </summary>
        [CommandMethod("CADAPI")]
        public void ExecuteCADAPI()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // 提示用戶輸入命令
            PromptStringOptions pso = new PromptStringOptions("\n請輸入命令 (例如: DRAW LINE 0,0 2000 HORIZONTAL): ");
            pso.AllowSpaces = true;
            PromptResult pr = ed.GetString(pso);

            if (pr.Status != PromptStatus.OK)
                return;

            // 解析並執行命令
            try
            {
                ParseAndExecuteExtendedCommand(pr.StringResult, doc, db, ed);
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\n錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 快速繪製直線命令
        /// </summary>
        [CommandMethod("CADLINE")]
        public void DrawLineCommand()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // 獲取起點
            PromptPointOptions ppo1 = new PromptPointOptions("\n指定起點: ");
            PromptPointResult ppr1 = ed.GetPoint(ppo1);
            if (ppr1.Status != PromptStatus.OK) return;

            // 獲取長度
            PromptDoubleOptions pdo = new PromptDoubleOptions("\n指定長度: ");
            pdo.DefaultValue = 2000;
            pdo.UseDefaultValue = true;
            PromptDoubleResult pdr = ed.GetDouble(pdo);
            if (pdr.Status != PromptStatus.OK) return;

            // 獲取方向
            PromptKeywordOptions pko = new PromptKeywordOptions("\n指定方向");
            pko.Keywords.Add("水平Horizontal");
            pko.Keywords.Add("垂直Vertical");
            pko.Keywords.Default = "水平Horizontal";
            pko.AllowNone = true;
            PromptResult pkr = ed.GetKeywords(pko);

            bool isHorizontal = pkr.Status != PromptStatus.OK || pkr.StringResult.StartsWith("水平") || pkr.StringResult.StartsWith("H");

            // 計算終點
            Point3d endPoint = isHorizontal
                ? new Point3d(ppr1.Value.X + pdr.Value, ppr1.Value.Y, ppr1.Value.Z)
                : new Point3d(ppr1.Value.X, ppr1.Value.Y + pdr.Value, ppr1.Value.Z);

            // 繪製直線
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                Line line = new Line(ppr1.Value, endPoint);
                btr.AppendEntity(line);
                trans.AddNewlyCreatedDBObject(line, true);

                trans.Commit();
                ed.WriteMessage($"\n成功繪製直線: ({ppr1.Value.X:F2}, {ppr1.Value.Y:F2}) 到 ({endPoint.X:F2}, {endPoint.Y:F2})");
            }
        }

        /// <summary>
        /// 解析並執行命令
        /// </summary>
        private void ParseAndExecuteCommand(string commandString, Document doc, Database db, Editor ed)
        {
            // 分割命令字符串
            var parts = commandString.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length < 4)
            {
                ed.WriteMessage("\n命令格式錯誤。格式: DRAW LINE 起點X,起點Y 終點X,終點Y 或 DRAW LINE 起點X,起點Y 長度 [方向]");
                return;
            }

            string action = parts[0].ToUpper();
            string type = parts[1].ToUpper();

            if (action != "DRAW")
            {
                ed.WriteMessage($"\n不支持的操作: {action}");
                return;
            }

            if (type != "LINE")
            {
                ed.WriteMessage($"\n不支持的類型: {type}");
                return;
            }

            // 解析起點
            var startPointMatch = Regex.Match(parts[2], @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");
            if (!startPointMatch.Success)
            {
                ed.WriteMessage($"\n無效的點格式: {parts[2]}");
                return;
            }

            double startX = double.Parse(startPointMatch.Groups[1].Value);
            double startY = double.Parse(startPointMatch.Groups[2].Value);

            double endX, endY;

            // 檢查第四個參數是否為終點坐標
            var endPointMatch = Regex.Match(parts[3], @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");
            if (endPointMatch.Success)
            {
                // 格式: DRAW LINE 起點X,起點Y 終點X,終點Y
                endX = double.Parse(endPointMatch.Groups[1].Value);
                endY = double.Parse(endPointMatch.Groups[2].Value);
            }
            else
            {
                // 格式: DRAW LINE 起點X,起點Y 長度 [方向]
                if (!double.TryParse(parts[3], out double length))
                {
                    ed.WriteMessage($"\n無效的長度: {parts[3]}");
                    return;
                }

                // 解析方向
                string direction = parts.Length > 4 ? parts[4].ToUpper() : "HORIZONTAL";
                
                // 計算終點
                endX = startX;
                endY = startY;

                switch (direction)
                {
                    case "HORIZONTAL":
                    case "H":
                        endX = startX + length;
                        break;
                    case "VERTICAL":
                    case "V":
                        endY = startY + length;
                        break;
                    default:
                        ed.WriteMessage($"\n不支持的方向: {direction}");
                        return;
                }
            }

            // 繪製直線
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                Point3d startPt = new Point3d(startX, startY, 0);
                Point3d endPt = new Point3d(endX, endY, 0);

                Line line = new Line(startPt, endPt);
                btr.AppendEntity(line);
                trans.AddNewlyCreatedDBObject(line, true);

                trans.Commit();
                ed.WriteMessage($"\n成功繪製直線: ({startX}, {startY}) 到 ({endX}, {endY})");
            }
        }

        /// <summary>
        /// 連續執行命令模式
        /// </summary>
        [CommandMethod("CADAPI_BATCH")]
        public void BatchMode()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            ed.WriteMessage("\n進入批次命令模式。輸入 EXIT 退出。");
            
            while (true)
            {
                PromptStringOptions pso = new PromptStringOptions("\nCAD> ");
                pso.AllowSpaces = true;
                PromptResult pr = ed.GetString(pso);

                if (pr.Status != PromptStatus.OK)
                    break;

                if (pr.StringResult.ToUpper() == "EXIT")
                    break;

                try
                {
                    ParseAndExecuteCommand(pr.StringResult, doc, db, ed);
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage($"\n錯誤: {ex.Message}");
                }
            }

            ed.WriteMessage("\n已退出批次命令模式。");
        }
    }
}