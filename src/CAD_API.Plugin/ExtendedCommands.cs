using System;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace CAD_API.Plugin
{
    public partial class CADCommands
    {
        /// <summary>
        /// 擴展的命令解析（支持圓形和顏色）
        /// </summary>
        public void ParseAndExecuteExtendedCommand(string commandString, Document doc, Database db, Editor ed)
        {
            var parts = commandString.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length < 2)
            {
                ed.WriteMessage("\n命令格式錯誤");
                return;
            }

            string action = parts[0].ToUpper();
            string type = parts[1].ToUpper();

            if (action == "DRAW")
            {
                switch (type)
                {
                    case "CIRCLE":
                        DrawCircleCommand(parts, doc, db, ed);
                        break;
                    case "LINE":
                        ParseAndExecuteCommand(commandString, doc, db, ed);
                        break;
                    case "POLYLINE":
                        DrawPolylineCommand(parts, doc, db, ed);
                        break;
                    default:
                        ed.WriteMessage($"\n不支持的繪圖類型: {type}");
                        break;
                }
            }
        }

        /// <summary>
        /// 繪製圓形
        /// </summary>
        private void DrawCircleCommand(string[] parts, Document doc, Database db, Editor ed)
        {
            // 格式: DRAW CIRCLE centerX,centerY radius [color]
            if (parts.Length < 4)
            {
                ed.WriteMessage("\n圓形命令格式: DRAW CIRCLE 中心X,中心Y 半徑 [顏色]");
                return;
            }

            // 解析中心點
            var centerMatch = Regex.Match(parts[2], @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");
            if (!centerMatch.Success)
            {
                ed.WriteMessage($"\n無效的中心點: {parts[2]}");
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

            // 解析顏色（可選）
            short colorIndex = 7; // 默認白色
            if (parts.Length > 4)
            {
                colorIndex = ParseColorName(parts[4]);
            }

            // 繪製圓形
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                Circle circle = new Circle(new Point3d(centerX, centerY, 0), Vector3d.ZAxis, radius);
                circle.ColorIndex = colorIndex;
                
                btr.AppendEntity(circle);
                trans.AddNewlyCreatedDBObject(circle, true);

                trans.Commit();
                ed.WriteMessage($"\n成功繪製圓形: 中心({centerX}, {centerY}), 半徑{radius}, 顏色索引{colorIndex}");
            }
        }

        /// <summary>
        /// 繪製多段線（用於近似圓形）
        /// </summary>
        private void DrawPolylineCommand(string[] parts, Document doc, Database db, Editor ed)
        {
            // 格式: DRAW POLYLINE centerX,centerY radius segments [color]
            if (parts.Length < 5)
            {
                ed.WriteMessage("\n多段線圓形命令格式: DRAW POLYLINE 中心X,中心Y 半徑 段數 [顏色]");
                return;
            }

            // 解析參數
            var centerMatch = Regex.Match(parts[2], @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");
            if (!centerMatch.Success) return;

            double centerX = double.Parse(centerMatch.Groups[1].Value);
            double centerY = double.Parse(centerMatch.Groups[2].Value);
            double radius = double.Parse(parts[3]);
            int segments = int.Parse(parts[4]);
            
            short colorIndex = 7;
            if (parts.Length > 5)
            {
                colorIndex = ParseColorName(parts[5]);
            }

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // 創建多段線點
                Polyline pline = new Polyline();
                double angleStep = 2 * Math.PI / segments;
                
                for (int i = 0; i <= segments; i++)
                {
                    double angle = i * angleStep;
                    double x = centerX + radius * Math.Cos(angle);
                    double y = centerY + radius * Math.Sin(angle);
                    pline.AddVertexAt(i, new Point2d(x, y), 0, 0, 0);
                }

                pline.Closed = true;
                pline.ColorIndex = colorIndex;
                
                btr.AppendEntity(pline);
                trans.AddNewlyCreatedDBObject(pline, true);

                trans.Commit();
                ed.WriteMessage($"\n成功繪製多段線圓形");
            }
        }

        /// <summary>
        /// 解析顏色名稱
        /// </summary>
        private short ParseColorName(string colorName)
        {
            switch (colorName.ToUpper())
            {
                case "RED": return 1;
                case "YELLOW": return 2;
                case "GREEN": return 3;
                case "CYAN": return 4;
                case "BLUE": return 5;
                case "MAGENTA": return 6;
                case "WHITE": return 7;
                case "ORANGE": return 30;
                case "BROWN": return 32;
                case "GRAY": case "GREY": return 8;
                default: return 7; // 默認白色
            }
        }
    }
}