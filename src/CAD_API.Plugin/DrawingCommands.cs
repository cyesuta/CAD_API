using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CAD_API.Plugin
{
    /// <summary>
    /// 擴展繪圖命令
    /// </summary>
    public partial class CADCommands
    {
        /// <summary>
        /// 更新的解析並執行命令方法，支援更多繪圖類型
        /// </summary>
        private void ParseAndExecuteExtendedCommand(string commandString, Document doc, Database db, Editor ed)
        {
            // 分割命令字符串
            var parts = commandString.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length < 3)
            {
                ed.WriteMessage("\n命令格式錯誤。");
                return;
            }

            string action = parts[0].ToUpper();
            string type = parts[1].ToUpper();

            // 處理修改命令
            switch (action)
            {
                case "SELECT":
                    SelectCommand(parts, doc, db, ed);
                    return;
                case "MOVE":
                    MoveCommand(parts, doc, db, ed);
                    return;
                case "COPY":
                    CopyCommand(parts, doc, db, ed);
                    return;
                case "ROTATE":
                    RotateCommand(parts, doc, db, ed);
                    return;
                case "SCALE":
                    ScaleCommand(parts, doc, db, ed);
                    return;
                case "TRIM":
                    TrimCommand(parts, doc, db, ed);
                    return;
                case "EXTEND":
                    ExtendCommand(parts, doc, db, ed);
                    return;
                case "OFFSET":
                    OffsetCommand(parts, doc, db, ed);
                    return;
            }

            if (action != "DRAW")
            {
                ed.WriteMessage($"\n不支持的操作: {action}");
                return;
            }

            // 根據類型調用對應的繪圖方法
            switch (type)
            {
                case "LINE":
                    ParseAndExecuteCommand(commandString, doc, db, ed); // 使用原有的直線繪製
                    break;
                case "CIRCLE":
                    DrawCircle(parts, doc, db, ed);
                    break;
                case "ARC":
                    DrawArc(parts, doc, db, ed);
                    break;
                case "RECTANGLE":
                case "RECT":
                    DrawRectangle(parts, doc, db, ed);
                    break;
                case "POLYLINE":
                case "PLINE":
                    DrawPolyline(parts, doc, db, ed);
                    break;
                case "TEXT":
                    DrawText(parts, doc, db, ed);
                    break;
                case "DIMENSION":
                case "DIM":
                    DrawDimension(parts, doc, db, ed);
                    break;
                case "HATCH":
                    DrawHatch(parts, doc, db, ed);
                    break;
                default:
                    ed.WriteMessage($"\n不支持的類型: {type}");
                    break;
            }
        }

        /// <summary>
        /// 繪製圓形
        /// 格式: DRAW CIRCLE 中心X,中心Y 半徑
        /// </summary>
        private void DrawCircle(string[] parts, Document doc, Database db, Editor ed)
        {
            if (parts.Length < 4)
            {
                ed.WriteMessage("\n命令格式錯誤。格式: DRAW CIRCLE 中心X,中心Y 半徑");
                return;
            }

            // 解析中心點
            var centerMatch = Regex.Match(parts[2], @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");
            if (!centerMatch.Success)
            {
                ed.WriteMessage($"\n無效的點格式: {parts[2]}");
                return;
            }

            double centerX = double.Parse(centerMatch.Groups[1].Value);
            double centerY = double.Parse(centerMatch.Groups[2].Value);

            // 解析半徑
            if (!double.TryParse(parts[3], out double radius))
            {
                ed.WriteMessage($"\n無效的半徑: {parts[3]}");
                return;
            }

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                Circle circle = new Circle(new Point3d(centerX, centerY, 0), Vector3d.ZAxis, radius);
                btr.AppendEntity(circle);
                trans.AddNewlyCreatedDBObject(circle, true);

                trans.Commit();
                ed.WriteMessage($"\n成功繪製圓形: 中心({centerX}, {centerY}) 半徑{radius}");
            }
        }

        /// <summary>
        /// 繪製弧線
        /// 格式: DRAW ARC 中心X,中心Y 半徑 起始角度 結束角度
        /// </summary>
        private void DrawArc(string[] parts, Document doc, Database db, Editor ed)
        {
            if (parts.Length < 6)
            {
                ed.WriteMessage("\n命令格式錯誤。格式: DRAW ARC 中心X,中心Y 半徑 起始角度 結束角度");
                return;
            }

            // 解析中心點
            var centerMatch = Regex.Match(parts[2], @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");
            if (!centerMatch.Success)
            {
                ed.WriteMessage($"\n無效的點格式: {parts[2]}");
                return;
            }

            double centerX = double.Parse(centerMatch.Groups[1].Value);
            double centerY = double.Parse(centerMatch.Groups[2].Value);

            // 解析半徑
            if (!double.TryParse(parts[3], out double radius))
            {
                ed.WriteMessage($"\n無效的半徑: {parts[3]}");
                return;
            }

            // 解析角度（轉換為弧度）
            if (!double.TryParse(parts[4], out double startAngle))
            {
                ed.WriteMessage($"\n無效的起始角度: {parts[4]}");
                return;
            }

            if (!double.TryParse(parts[5], out double endAngle))
            {
                ed.WriteMessage($"\n無效的結束角度: {parts[5]}");
                return;
            }

            // 轉換角度為弧度
            startAngle = startAngle * Math.PI / 180.0;
            endAngle = endAngle * Math.PI / 180.0;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                Arc arc = new Arc(new Point3d(centerX, centerY, 0), Vector3d.ZAxis, radius, startAngle, endAngle);
                btr.AppendEntity(arc);
                trans.AddNewlyCreatedDBObject(arc, true);

                trans.Commit();
                ed.WriteMessage($"\n成功繪製弧線: 中心({centerX}, {centerY}) 半徑{radius} 角度{parts[4]}°-{parts[5]}°");
            }
        }

        /// <summary>
        /// 繪製矩形
        /// 格式: DRAW RECTANGLE 角點1X,角點1Y 角點2X,角點2Y
        /// 或: DRAW RECTANGLE 角點1X,角點1Y 寬度 高度
        /// </summary>
        private void DrawRectangle(string[] parts, Document doc, Database db, Editor ed)
        {
            if (parts.Length < 4)
            {
                ed.WriteMessage("\n命令格式錯誤。格式: DRAW RECTANGLE 角點1X,角點1Y 角點2X,角點2Y 或 DRAW RECTANGLE 角點1X,角點1Y 寬度 高度");
                return;
            }

            // 解析第一個角點
            var corner1Match = Regex.Match(parts[2], @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");
            if (!corner1Match.Success)
            {
                ed.WriteMessage($"\n無效的點格式: {parts[2]}");
                return;
            }

            double x1 = double.Parse(corner1Match.Groups[1].Value);
            double y1 = double.Parse(corner1Match.Groups[2].Value);

            double x2, y2;

            // 檢查第二個參數是否為點
            var corner2Match = Regex.Match(parts[3], @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");
            if (corner2Match.Success)
            {
                // 格式: 角點1 角點2
                x2 = double.Parse(corner2Match.Groups[1].Value);
                y2 = double.Parse(corner2Match.Groups[2].Value);
            }
            else
            {
                // 格式: 角點1 寬度 高度
                if (parts.Length < 5)
                {
                    ed.WriteMessage("\n命令格式錯誤。需要寬度和高度參數");
                    return;
                }

                if (!double.TryParse(parts[3], out double width))
                {
                    ed.WriteMessage($"\n無效的寬度: {parts[3]}");
                    return;
                }

                if (!double.TryParse(parts[4], out double height))
                {
                    ed.WriteMessage($"\n無效的高度: {parts[4]}");
                    return;
                }

                x2 = x1 + width;
                y2 = y1 + height;
            }

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // 創建多段線來繪製矩形
                Polyline pline = new Polyline();
                pline.AddVertexAt(0, new Point2d(x1, y1), 0, 0, 0);
                pline.AddVertexAt(1, new Point2d(x2, y1), 0, 0, 0);
                pline.AddVertexAt(2, new Point2d(x2, y2), 0, 0, 0);
                pline.AddVertexAt(3, new Point2d(x1, y2), 0, 0, 0);
                pline.Closed = true;

                btr.AppendEntity(pline);
                trans.AddNewlyCreatedDBObject(pline, true);

                trans.Commit();
                ed.WriteMessage($"\n成功繪製矩形: ({x1}, {y1}) 到 ({x2}, {y2})");
            }
        }

        /// <summary>
        /// 繪製多段線
        /// 格式: DRAW POLYLINE 點1X,點1Y 點2X,點2Y 點3X,點3Y ... [CLOSED]
        /// </summary>
        private void DrawPolyline(string[] parts, Document doc, Database db, Editor ed)
        {
            if (parts.Length < 4)
            {
                ed.WriteMessage("\n命令格式錯誤。格式: DRAW POLYLINE 點1X,點1Y 點2X,點2Y ... [CLOSED]");
                return;
            }

            List<Point2d> points = new List<Point2d>();
            bool isClosed = false;

            // 解析所有點
            for (int i = 2; i < parts.Length; i++)
            {
                if (parts[i].ToUpper() == "CLOSED")
                {
                    isClosed = true;
                    break;
                }

                var pointMatch = Regex.Match(parts[i], @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");
                if (!pointMatch.Success)
                {
                    ed.WriteMessage($"\n無效的點格式: {parts[i]}");
                    return;
                }

                double x = double.Parse(pointMatch.Groups[1].Value);
                double y = double.Parse(pointMatch.Groups[2].Value);
                points.Add(new Point2d(x, y));
            }

            if (points.Count < 2)
            {
                ed.WriteMessage("\n至少需要兩個點");
                return;
            }

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                Polyline pline = new Polyline();
                for (int i = 0; i < points.Count; i++)
                {
                    pline.AddVertexAt(i, points[i], 0, 0, 0);
                }
                pline.Closed = isClosed;

                btr.AppendEntity(pline);
                trans.AddNewlyCreatedDBObject(pline, true);

                trans.Commit();
                ed.WriteMessage($"\n成功繪製多段線: {points.Count} 個頂點{(isClosed ? " (封閉)" : "")}");
            }
        }

        /// <summary>
        /// 添加文字
        /// 格式: DRAW TEXT 位置X,位置Y "文字內容" [高度]
        /// </summary>
        private void DrawText(string[] parts, Document doc, Database db, Editor ed)
        {
            if (parts.Length < 4)
            {
                ed.WriteMessage("\n命令格式錯誤。格式: DRAW TEXT 位置X,位置Y \"文字內容\" [高度]");
                return;
            }

            // 解析位置
            var posMatch = Regex.Match(parts[2], @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");
            if (!posMatch.Success)
            {
                ed.WriteMessage($"\n無效的點格式: {parts[2]}");
                return;
            }

            double x = double.Parse(posMatch.Groups[1].Value);
            double y = double.Parse(posMatch.Groups[2].Value);

            // 提取文字內容（支援引號內的空格）
            string textContent = "";
            double height = 100; // 預設高度

            // 檢查是否有引號包圍的文字
            string remainingCommand = string.Join(" ", parts, 3, parts.Length - 3);
            var textMatch = Regex.Match(remainingCommand, @"^""([^""]+)""(?:\s+(\d+(?:\.\d+)?))?");
            
            if (textMatch.Success)
            {
                textContent = textMatch.Groups[1].Value;
                if (textMatch.Groups[2].Success)
                {
                    double.TryParse(textMatch.Groups[2].Value, out height);
                }
            }
            else
            {
                // 沒有引號，使用單個詞作為文字
                textContent = parts[3];
                if (parts.Length > 4)
                {
                    double.TryParse(parts[4], out height);
                }
            }

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                DBText text = new DBText();
                text.Position = new Point3d(x, y, 0);
                text.Height = height;
                text.TextString = textContent;

                btr.AppendEntity(text);
                trans.AddNewlyCreatedDBObject(text, true);

                trans.Commit();
                ed.WriteMessage($"\n成功添加文字: \"{textContent}\" 在 ({x}, {y}) 高度 {height}");
            }
        }

        /// <summary>
        /// 添加尺寸標註
        /// 格式: DRAW DIMENSION 點1X,點1Y 點2X,點2Y [偏移距離]
        /// </summary>
        private void DrawDimension(string[] parts, Document doc, Database db, Editor ed)
        {
            if (parts.Length < 4)
            {
                ed.WriteMessage("\n命令格式錯誤。格式: DRAW DIMENSION 點1X,點1Y 點2X,點2Y [偏移距離]");
                return;
            }

            // 解析點1
            var point1Match = Regex.Match(parts[2], @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");
            if (!point1Match.Success)
            {
                ed.WriteMessage($"\n無效的點格式: {parts[2]}");
                return;
            }

            double x1 = double.Parse(point1Match.Groups[1].Value);
            double y1 = double.Parse(point1Match.Groups[2].Value);

            // 解析點2
            var point2Match = Regex.Match(parts[3], @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");
            if (!point2Match.Success)
            {
                ed.WriteMessage($"\n無效的點格式: {parts[3]}");
                return;
            }

            double x2 = double.Parse(point2Match.Groups[1].Value);
            double y2 = double.Parse(point2Match.Groups[2].Value);

            // 偏移距離
            double offset = 500; // 預設偏移
            if (parts.Length > 4)
            {
                double.TryParse(parts[4], out offset);
            }

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // 創建對齊尺寸標註
                AlignedDimension dim = new AlignedDimension();
                dim.XLine1Point = new Point3d(x1, y1, 0);
                dim.XLine2Point = new Point3d(x2, y2, 0);
                
                // 計算尺寸線位置（垂直於測量線）
                Vector3d direction = new Vector3d(x2 - x1, y2 - y1, 0);
                Vector3d perpendicular = direction.GetPerpendicularVector().GetNormal();
                Point3d dimLinePoint = new Point3d((x1 + x2) / 2, (y1 + y2) / 2, 0) + perpendicular * offset;
                dim.DimLinePoint = dimLinePoint;

                // 使用當前尺寸樣式
                dim.DimensionStyle = db.Dimstyle;

                btr.AppendEntity(dim);
                trans.AddNewlyCreatedDBObject(dim, true);

                trans.Commit();
                ed.WriteMessage($"\n成功添加尺寸標註: ({x1}, {y1}) 到 ({x2}, {y2})");
            }
        }

        /// <summary>
        /// 添加填充
        /// 格式: DRAW HATCH PATTERN 邊界點1X,邊界點1Y 邊界點2X,邊界點2Y ...
        /// </summary>
        private void DrawHatch(string[] parts, Document doc, Database db, Editor ed)
        {
            if (parts.Length < 5)
            {
                ed.WriteMessage("\n命令格式錯誤。格式: DRAW HATCH PATTERN 邊界點1X,邊界點1Y 邊界點2X,邊界點2Y ...");
                return;
            }

            string pattern = parts[2].ToUpper();
            if (pattern != "SOLID" && pattern != "ANSI31" && pattern != "ANSI32" && pattern != "DOTS")
            {
                pattern = "SOLID"; // 預設實心填充
            }

            List<Point2d> points = new List<Point2d>();

            // 解析邊界點
            for (int i = 3; i < parts.Length; i++)
            {
                var pointMatch = Regex.Match(parts[i], @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");
                if (!pointMatch.Success)
                {
                    ed.WriteMessage($"\n無效的點格式: {parts[i]}");
                    return;
                }

                double x = double.Parse(pointMatch.Groups[1].Value);
                double y = double.Parse(pointMatch.Groups[2].Value);
                points.Add(new Point2d(x, y));
            }

            if (points.Count < 3)
            {
                ed.WriteMessage("\n至少需要三個點來定義邊界");
                return;
            }

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // 創建邊界多段線
                Polyline boundary = new Polyline();
                for (int i = 0; i < points.Count; i++)
                {
                    boundary.AddVertexAt(i, points[i], 0, 0, 0);
                }
                boundary.Closed = true;

                ObjectId polyId = btr.AppendEntity(boundary);
                trans.AddNewlyCreatedDBObject(boundary, true);

                // 創建填充
                Hatch hatch = new Hatch();
                hatch.SetDatabaseDefaults();

                if (pattern == "SOLID")
                {
                    hatch.PatternScale = 1.0;
                    hatch.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");
                }
                else
                {
                    hatch.PatternScale = 25.0; // 調整圖案比例
                    hatch.SetHatchPattern(HatchPatternType.PreDefined, pattern);
                }

                btr.AppendEntity(hatch);
                trans.AddNewlyCreatedDBObject(hatch, true);

                // 添加邊界到填充
                ObjectIdCollection boundaryIds = new ObjectIdCollection();
                boundaryIds.Add(polyId);
                hatch.AppendLoop(HatchLoopTypes.External, boundaryIds);
                hatch.EvaluateHatch(true);

                trans.Commit();
                ed.WriteMessage($"\n成功添加填充: 圖案 {pattern}，{points.Count} 個邊界點");
            }
        }

        /// <summary>
        /// 更新批次模式以支援新命令
        /// </summary>
        [CommandMethod("CADAPI_BATCH_EX")]
        public void ExtendedBatchMode()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            ed.WriteMessage("\n進入擴展批次命令模式。輸入 EXIT 退出。");
            ed.WriteMessage("\n支援的命令: LINE, CIRCLE, ARC, RECTANGLE, POLYLINE, TEXT, DIMENSION, HATCH");
            
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
                    ParseAndExecuteExtendedCommand(pr.StringResult, doc, db, ed);
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage($"\n錯誤: {ex.Message}");
                }
            }

            ed.WriteMessage("\n已退出擴展批次命令模式。");
        }
    }
}