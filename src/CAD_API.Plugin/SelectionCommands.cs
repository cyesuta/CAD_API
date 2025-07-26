using Autodesk.AutoCAD.ApplicationServices;
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
    /// 選擇命令和選擇集管理
    /// </summary>
    public partial class CADCommands
    {
        // 儲存當前選擇集
        private static ObjectIdCollection _currentSelection = new ObjectIdCollection();
        
        /// <summary>
        /// 獲取當前選擇集
        /// </summary>
        private ObjectIdCollection GetSelection()
        {
            return _currentSelection;
        }

        /// <summary>
        /// 選擇物件命令
        /// 格式: 
        /// SELECT ALL - 選擇所有物件
        /// SELECT WINDOW 點1X,點1Y 點2X,點2Y - 窗口選擇
        /// SELECT CROSSING 點1X,點1Y 點2X,點2Y - 交叉窗口選擇
        /// SELECT POINT X,Y [容差] - 點選擇
        /// SELECT TYPE 類型 - 按類型選擇（LINE, CIRCLE, ARC等）
        /// SELECT LAYER 圖層名 - 按圖層選擇
        /// SELECT CLEAR - 清除選擇集
        /// </summary>
        private void SelectCommand(string[] parts, Document doc, Database db, Editor ed)
        {
            if (parts.Length < 2)
            {
                ed.WriteMessage("\n命令格式錯誤。使用 SELECT ALL/WINDOW/CROSSING/POINT/TYPE/LAYER/CLEAR");
                return;
            }

            string mode = parts[1].ToUpper();

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                try
                {
                    _currentSelection.Clear();
                    PromptSelectionResult selResult = null;

                    switch (mode)
                    {
                        case "ALL":
                            // 選擇所有物件
                            selResult = ed.SelectAll();
                            break;

                        case "WINDOW":
                            if (parts.Length < 4)
                            {
                                ed.WriteMessage("\n需要兩個點座標");
                                return;
                            }
                            selResult = SelectByWindow(parts[2], parts[3], ed);
                            break;

                        case "CROSSING":
                            if (parts.Length < 4)
                            {
                                ed.WriteMessage("\n需要兩個點座標");
                                return;
                            }
                            selResult = SelectByCrossing(parts[2], parts[3], ed);
                            break;

                        case "POINT":
                            if (parts.Length < 3)
                            {
                                ed.WriteMessage("\n需要點座標");
                                return;
                            }
                            double tolerance = parts.Length > 3 ? double.Parse(parts[3]) : 10.0;
                            selResult = SelectByPoint(parts[2], tolerance, ed);
                            break;

                        case "TYPE":
                            if (parts.Length < 3)
                            {
                                ed.WriteMessage("\n需要指定類型");
                                return;
                            }
                            selResult = SelectByType(parts[2], ed);
                            break;

                        case "LAYER":
                            if (parts.Length < 3)
                            {
                                ed.WriteMessage("\n需要指定圖層名");
                                return;
                            }
                            selResult = SelectByLayer(parts[2], ed);
                            break;

                        case "CLEAR":
                            _currentSelection.Clear();
                            ed.WriteMessage("\n選擇集已清除");
                            trans.Commit();
                            return;

                        default:
                            ed.WriteMessage($"\n不支援的選擇模式: {mode}");
                            return;
                    }

                    if (selResult != null && selResult.Status == PromptStatus.OK)
                    {
                        foreach (ObjectId id in selResult.Value.GetObjectIds())
                        {
                            _currentSelection.Add(id);
                        }
                        ed.WriteMessage($"\n已選擇 {_currentSelection.Count} 個物件");
                    }
                    else
                    {
                        ed.WriteMessage("\n沒有選擇到物件");
                    }

                    trans.Commit();
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage($"\n選擇失敗: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 窗口選擇
        /// </summary>
        private PromptSelectionResult SelectByWindow(string pt1Str, string pt2Str, Editor ed)
        {
            var pt1Match = Regex.Match(pt1Str, @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");
            var pt2Match = Regex.Match(pt2Str, @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");

            if (!pt1Match.Success || !pt2Match.Success)
            {
                ed.WriteMessage("\n無效的點格式");
                return null;
            }

            Point3d pt1 = new Point3d(double.Parse(pt1Match.Groups[1].Value),
                                     double.Parse(pt1Match.Groups[2].Value), 0);
            Point3d pt2 = new Point3d(double.Parse(pt2Match.Groups[1].Value),
                                     double.Parse(pt2Match.Groups[2].Value), 0);

            return ed.SelectWindow(pt1, pt2);
        }

        /// <summary>
        /// 交叉窗口選擇
        /// </summary>
        private PromptSelectionResult SelectByCrossing(string pt1Str, string pt2Str, Editor ed)
        {
            var pt1Match = Regex.Match(pt1Str, @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");
            var pt2Match = Regex.Match(pt2Str, @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");

            if (!pt1Match.Success || !pt2Match.Success)
            {
                ed.WriteMessage("\n無效的點格式");
                return null;
            }

            Point3d pt1 = new Point3d(double.Parse(pt1Match.Groups[1].Value),
                                     double.Parse(pt1Match.Groups[2].Value), 0);
            Point3d pt2 = new Point3d(double.Parse(pt2Match.Groups[1].Value),
                                     double.Parse(pt2Match.Groups[2].Value), 0);

            return ed.SelectCrossingWindow(pt1, pt2);
        }

        /// <summary>
        /// 點選擇
        /// </summary>
        private PromptSelectionResult SelectByPoint(string ptStr, double tolerance, Editor ed)
        {
            var ptMatch = Regex.Match(ptStr, @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");
            if (!ptMatch.Success)
            {
                ed.WriteMessage("\n無效的點格式");
                return null;
            }

            Point3d pt = new Point3d(double.Parse(ptMatch.Groups[1].Value),
                                    double.Parse(ptMatch.Groups[2].Value), 0);

            // 創建一個小的選擇框
            Point3d pt1 = new Point3d(pt.X - tolerance, pt.Y - tolerance, 0);
            Point3d pt2 = new Point3d(pt.X + tolerance, pt.Y + tolerance, 0);

            return ed.SelectCrossingWindow(pt1, pt2);
        }

        /// <summary>
        /// 按類型選擇
        /// </summary>
        private PromptSelectionResult SelectByType(string typeName, Editor ed)
        {
            // 創建類型過濾器
            TypedValue[] filterList = null;

            switch (typeName.ToUpper())
            {
                case "LINE":
                    filterList = new TypedValue[] { new TypedValue((int)DxfCode.Start, "LINE") };
                    break;
                case "CIRCLE":
                    filterList = new TypedValue[] { new TypedValue((int)DxfCode.Start, "CIRCLE") };
                    break;
                case "ARC":
                    filterList = new TypedValue[] { new TypedValue((int)DxfCode.Start, "ARC") };
                    break;
                case "POLYLINE":
                case "PLINE":
                    filterList = new TypedValue[] { new TypedValue((int)DxfCode.Start, "LWPOLYLINE") };
                    break;
                case "TEXT":
                    filterList = new TypedValue[] { 
                        new TypedValue((int)DxfCode.Operator, "<OR"),
                        new TypedValue((int)DxfCode.Start, "TEXT"),
                        new TypedValue((int)DxfCode.Start, "MTEXT"),
                        new TypedValue((int)DxfCode.Operator, "OR>")
                    };
                    break;
                case "DIMENSION":
                case "DIM":
                    filterList = new TypedValue[] { new TypedValue((int)DxfCode.Start, "DIMENSION") };
                    break;
                case "HATCH":
                    filterList = new TypedValue[] { new TypedValue((int)DxfCode.Start, "HATCH") };
                    break;
                default:
                    ed.WriteMessage($"\n不支援的類型: {typeName}");
                    return null;
            }

            SelectionFilter filter = new SelectionFilter(filterList);
            return ed.SelectAll(filter);
        }

        /// <summary>
        /// 按圖層選擇
        /// </summary>
        private PromptSelectionResult SelectByLayer(string layerName, Editor ed)
        {
            TypedValue[] filterList = new TypedValue[] {
                new TypedValue((int)DxfCode.LayerName, layerName)
            };

            SelectionFilter filter = new SelectionFilter(filterList);
            return ed.SelectAll(filter);
        }

        /// <summary>
        /// 更新修改命令以支援選擇集
        /// </summary>
        private ObjectIdCollection GetTargetObjects(string targetSpec, Transaction trans, Database db, Editor ed)
        {
            ObjectIdCollection targets = new ObjectIdCollection();

            if (targetSpec.ToUpper() == "LAST")
            {
                // 獲取最後一個物件
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                
                ObjectId lastId = ObjectId.Null;
                foreach (ObjectId id in btr)
                {
                    lastId = id;
                }
                
                if (lastId != ObjectId.Null)
                {
                    targets.Add(lastId);
                }
            }
            else if (targetSpec.ToUpper() == "SELECTED" || targetSpec.ToUpper() == "SEL")
            {
                // 使用當前選擇集
                foreach (ObjectId id in _currentSelection)
                {
                    targets.Add(id);
                }
            }
            else if (targetSpec.ToUpper() == "ALL")
            {
                // 選擇所有物件
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                
                foreach (ObjectId id in btr)
                {
                    Entity ent = trans.GetObject(id, OpenMode.ForRead) as Entity;
                    if (ent != null && ent.Visible)
                    {
                        targets.Add(id);
                    }
                }
            }

            return targets;
        }
    }
}