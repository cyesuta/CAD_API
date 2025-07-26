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
    /// 修改命令實現
    /// </summary>
    public partial class CADCommands
    {
        /// <summary>
        /// 移動物件
        /// 格式: MOVE LAST/SELECTED/ALL 位移X,位移Y
        /// </summary>
        private void MoveCommand(string[] parts, Document doc, Database db, Editor ed)
        {
            if (parts.Length < 3)
            {
                ed.WriteMessage("\n命令格式錯誤。格式: MOVE LAST/SELECTED/ALL 位移X,位移Y");
                return;
            }

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                try
                {
                    // 獲取目標物件
                    ObjectIdCollection objectsToMove = GetTargetObjects(parts[1], trans, db, ed);

                    if (objectsToMove.Count == 0)
                    {
                        ed.WriteMessage("\n沒有找到要移動的物件");
                        return;
                    }

                    // 解析位移向量
                    var dispMatch = Regex.Match(parts[parts.Length - 1], @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");
                    if (!dispMatch.Success)
                    {
                        ed.WriteMessage("\n無效的位移格式");
                        return;
                    }

                    double dx = double.Parse(dispMatch.Groups[1].Value);
                    double dy = double.Parse(dispMatch.Groups[2].Value);
                    Vector3d displacement = new Vector3d(dx, dy, 0);

                    // 移動物件
                    foreach (ObjectId id in objectsToMove)
                    {
                        Entity ent = trans.GetObject(id, OpenMode.ForWrite) as Entity;
                        ent.TransformBy(Matrix3d.Displacement(displacement));
                    }

                    trans.Commit();
                    ed.WriteMessage($"\n成功移動 {objectsToMove.Count} 個物件，位移 ({dx}, {dy})");
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage($"\n移動失敗: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 複製物件
        /// 格式: COPY LAST/SELECTED/ALL 位移X,位移Y [複製數量]
        /// </summary>
        private void CopyCommand(string[] parts, Document doc, Database db, Editor ed)
        {
            if (parts.Length < 3)
            {
                ed.WriteMessage("\n命令格式錯誤。格式: COPY LAST/SELECTED/ALL 位移X,位移Y [複製數量]");
                return;
            }

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                try
                {
                    // 獲取目標物件
                    ObjectIdCollection sourcesToCopy = GetTargetObjects(parts[1], trans, db, ed);

                    if (sourcesToCopy.Count == 0)
                    {
                        ed.WriteMessage("\n沒有找到要複製的物件");
                        return;
                    }

                    // 解析位移
                    var dispMatch = Regex.Match(parts[2], @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");
                    if (!dispMatch.Success)
                    {
                        ed.WriteMessage("\n無效的位移格式");
                        return;
                    }

                    double dx = double.Parse(dispMatch.Groups[1].Value);
                    double dy = double.Parse(dispMatch.Groups[2].Value);
                    
                    // 複製數量（預設1）
                    int copies = 1;
                    if (parts.Length > 3)
                    {
                        int.TryParse(parts[3], out copies);
                        copies = Math.Max(1, Math.Min(copies, 100)); // 限制在1-100之間
                    }

                    // 執行複製
                    BlockTableRecord btr = trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                    int totalCopied = 0;

                    foreach (ObjectId sourceId in sourcesToCopy)
                    {
                        Entity sourceEnt = trans.GetObject(sourceId, OpenMode.ForRead) as Entity;
                        
                        for (int i = 1; i <= copies; i++)
                        {
                            Entity newEnt = sourceEnt.Clone() as Entity;
                            Vector3d displacement = new Vector3d(dx * i, dy * i, 0);
                            newEnt.TransformBy(Matrix3d.Displacement(displacement));
                            btr.AppendEntity(newEnt);
                            trans.AddNewlyCreatedDBObject(newEnt, true);
                            totalCopied++;
                        }
                    }

                    trans.Commit();
                    ed.WriteMessage($"\n成功複製 {totalCopied} 個物件");
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage($"\n複製失敗: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 旋轉物件
        /// 格式: ROTATE LAST 中心X,中心Y 角度
        /// </summary>
        private void RotateCommand(string[] parts, Document doc, Database db, Editor ed)
        {
            if (parts.Length < 4)
            {
                ed.WriteMessage("\n命令格式錯誤。格式: ROTATE LAST 中心X,中心Y 角度");
                return;
            }

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                try
                {
                    ObjectId targetId = ObjectId.Null;

                    if (parts[1].ToUpper() == "LAST")
                    {
                        BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                        
                        foreach (ObjectId id in btr)
                        {
                            targetId = id;
                        }
                    }

                    if (targetId == ObjectId.Null)
                    {
                        ed.WriteMessage("\n沒有找到要旋轉的物件");
                        return;
                    }

                    // 解析旋轉中心
                    var centerMatch = Regex.Match(parts[2], @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");
                    if (!centerMatch.Success)
                    {
                        ed.WriteMessage("\n無效的中心點格式");
                        return;
                    }

                    double cx = double.Parse(centerMatch.Groups[1].Value);
                    double cy = double.Parse(centerMatch.Groups[2].Value);
                    Point3d center = new Point3d(cx, cy, 0);

                    // 解析角度
                    if (!double.TryParse(parts[3], out double angleDegrees))
                    {
                        ed.WriteMessage("\n無效的角度");
                        return;
                    }

                    double angleRadians = angleDegrees * Math.PI / 180.0;

                    // 執行旋轉
                    Entity ent = trans.GetObject(targetId, OpenMode.ForWrite) as Entity;
                    ent.TransformBy(Matrix3d.Rotation(angleRadians, Vector3d.ZAxis, center));

                    trans.Commit();
                    ed.WriteMessage($"\n成功旋轉物件 {angleDegrees} 度");
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage($"\n旋轉失敗: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 縮放物件
        /// 格式: SCALE LAST 基點X,基點Y 比例
        /// </summary>
        private void ScaleCommand(string[] parts, Document doc, Database db, Editor ed)
        {
            if (parts.Length < 4)
            {
                ed.WriteMessage("\n命令格式錯誤。格式: SCALE LAST 基點X,基點Y 比例");
                return;
            }

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                try
                {
                    ObjectId targetId = ObjectId.Null;

                    if (parts[1].ToUpper() == "LAST")
                    {
                        BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                        
                        foreach (ObjectId id in btr)
                        {
                            targetId = id;
                        }
                    }

                    if (targetId == ObjectId.Null)
                    {
                        ed.WriteMessage("\n沒有找到要縮放的物件");
                        return;
                    }

                    // 解析基點
                    var baseMatch = Regex.Match(parts[2], @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");
                    if (!baseMatch.Success)
                    {
                        ed.WriteMessage("\n無效的基點格式");
                        return;
                    }

                    double bx = double.Parse(baseMatch.Groups[1].Value);
                    double by = double.Parse(baseMatch.Groups[2].Value);
                    Point3d basePoint = new Point3d(bx, by, 0);

                    // 解析比例
                    if (!double.TryParse(parts[3], out double scale))
                    {
                        ed.WriteMessage("\n無效的縮放比例");
                        return;
                    }

                    // 執行縮放
                    Entity ent = trans.GetObject(targetId, OpenMode.ForWrite) as Entity;
                    ent.TransformBy(Matrix3d.Scaling(scale, basePoint));

                    trans.Commit();
                    ed.WriteMessage($"\n成功縮放物件，比例 {scale}");
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage($"\n縮放失敗: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 偏移複製
        /// 格式: OFFSET LAST 偏移距離 [方向點X,方向點Y]
        /// </summary>
        private void OffsetCommand(string[] parts, Document doc, Database db, Editor ed)
        {
            if (parts.Length < 3)
            {
                ed.WriteMessage("\n命令格式錯誤。格式: OFFSET LAST 偏移距離 [方向點X,方向點Y]");
                return;
            }

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                try
                {
                    ObjectId sourceId = ObjectId.Null;

                    if (parts[1].ToUpper() == "LAST")
                    {
                        BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                        
                        foreach (ObjectId id in btr)
                        {
                            Entity testEnt = trans.GetObject(id, OpenMode.ForRead) as Entity;
                            if (testEnt is Curve) // 只能偏移曲線類物件
                            {
                                sourceId = id;
                            }
                        }
                    }

                    if (sourceId == ObjectId.Null)
                    {
                        ed.WriteMessage("\n沒有找到可偏移的曲線物件");
                        return;
                    }

                    // 解析偏移距離
                    if (!double.TryParse(parts[2], out double offsetDist))
                    {
                        ed.WriteMessage("\n無效的偏移距離");
                        return;
                    }

                    Curve sourceCurve = trans.GetObject(sourceId, OpenMode.ForRead) as Curve;
                    BlockTableRecord btr2 = trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                    // 獲取偏移方向點
                    Point3d offsetPoint;
                    if (parts.Length > 3)
                    {
                        var ptMatch = Regex.Match(parts[3], @"^(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)$");
                        if (ptMatch.Success)
                        {
                            offsetPoint = new Point3d(double.Parse(ptMatch.Groups[1].Value),
                                                     double.Parse(ptMatch.Groups[2].Value), 0);
                        }
                        else
                        {
                            // 預設向右偏移
                            Point3d midPoint = sourceCurve.GetPointAtParameter(
                                (sourceCurve.StartParam + sourceCurve.EndParam) / 2);
                            offsetPoint = midPoint + new Vector3d(offsetDist, 0, 0);
                        }
                    }
                    else
                    {
                        // 預設向右偏移
                        Point3d midPoint = sourceCurve.GetPointAtParameter(
                            (sourceCurve.StartParam + sourceCurve.EndParam) / 2);
                        offsetPoint = midPoint + new Vector3d(offsetDist, 0, 0);
                    }

                    // 執行偏移
                    DBObjectCollection offsetCurves = sourceCurve.GetOffsetCurves(offsetDist);
                    
                    if (offsetCurves.Count > 0)
                    {
                        // 如果有多個偏移結果，選擇最接近指定點的
                        Entity bestOffset = null;
                        double minDist = double.MaxValue;
                        
                        foreach (Entity offsetEnt in offsetCurves)
                        {
                            if (offsetEnt is Curve offsetCurve)
                            {
                                Point3d closestPt = offsetCurve.GetClosestPointTo(offsetPoint, false);
                                double dist = closestPt.DistanceTo(offsetPoint);
                                if (dist < minDist)
                                {
                                    minDist = dist;
                                    bestOffset = offsetEnt;
                                }
                            }
                        }

                        if (bestOffset != null)
                        {
                            btr2.AppendEntity(bestOffset);
                            trans.AddNewlyCreatedDBObject(bestOffset, true);
                            ed.WriteMessage($"\n成功偏移物件，距離 {offsetDist}");
                        }
                    }

                    trans.Commit();
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage($"\n偏移失敗: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 修剪線段（簡化版 - 修剪到指定長度）
        /// 格式: TRIM LAST 新長度
        /// </summary>
        private void TrimCommand(string[] parts, Document doc, Database db, Editor ed)
        {
            if (parts.Length < 3)
            {
                ed.WriteMessage("\n命令格式錯誤。格式: TRIM LAST 新長度");
                return;
            }

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                try
                {
                    ObjectId targetId = ObjectId.Null;

                    if (parts[1].ToUpper() == "LAST")
                    {
                        BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                        
                        foreach (ObjectId id in btr)
                        {
                            Entity testEnt = trans.GetObject(id, OpenMode.ForRead) as Entity;
                            if (testEnt is Line)
                            {
                                targetId = id;
                            }
                        }
                    }

                    if (targetId == ObjectId.Null)
                    {
                        ed.WriteMessage("\n沒有找到要修剪的直線");
                        return;
                    }

                    // 解析新長度
                    if (!double.TryParse(parts[2], out double newLength))
                    {
                        ed.WriteMessage("\n無效的長度");
                        return;
                    }

                    // 修剪直線
                    Line line = trans.GetObject(targetId, OpenMode.ForWrite) as Line;
                    Vector3d direction = line.EndPoint - line.StartPoint;
                    direction = direction.GetNormal();
                    line.EndPoint = line.StartPoint + direction * newLength;

                    trans.Commit();
                    ed.WriteMessage($"\n成功修剪直線到長度 {newLength}");
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage($"\n修剪失敗: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 延伸線段
        /// 格式: EXTEND LAST 延伸長度
        /// </summary>
        private void ExtendCommand(string[] parts, Document doc, Database db, Editor ed)
        {
            if (parts.Length < 3)
            {
                ed.WriteMessage("\n命令格式錯誤。格式: EXTEND LAST 延伸長度");
                return;
            }

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                try
                {
                    ObjectId targetId = ObjectId.Null;

                    if (parts[1].ToUpper() == "LAST")
                    {
                        BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                        
                        foreach (ObjectId id in btr)
                        {
                            Entity testEnt = trans.GetObject(id, OpenMode.ForRead) as Entity;
                            if (testEnt is Line)
                            {
                                targetId = id;
                            }
                        }
                    }

                    if (targetId == ObjectId.Null)
                    {
                        ed.WriteMessage("\n沒有找到要延伸的直線");
                        return;
                    }

                    // 解析延伸長度
                    if (!double.TryParse(parts[2], out double extendLength))
                    {
                        ed.WriteMessage("\n無效的延伸長度");
                        return;
                    }

                    // 延伸直線
                    Line line = trans.GetObject(targetId, OpenMode.ForWrite) as Line;
                    Vector3d direction = line.EndPoint - line.StartPoint;
                    direction = direction.GetNormal();
                    line.EndPoint = line.EndPoint + direction * extendLength;

                    trans.Commit();
                    ed.WriteMessage($"\n成功延伸直線 {extendLength} 單位");
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage($"\n延伸失敗: {ex.Message}");
                }
            }
        }
    }
}