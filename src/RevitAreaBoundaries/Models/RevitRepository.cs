using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitAreaBoundaries.Services;

namespace RevitAreaBoundaries.Models;

internal class RevitRepository {
    private readonly OuterSquareService _outerSquareService;
    private readonly ElementSectionService _elementSectionService;
    private readonly CurveService _curveService;
    private readonly BoundaryFindService _boundaryFindService;
    private readonly double _sideCell = UnitUtils.ConvertToInternalUnits(1000, UnitTypeId.Millimeters);

    public RevitRepository(
        UIApplication uiApplication, 
        OuterSquareService outerSquareService, 
        ElementSectionService elementSectionService, 
        CurveService curveService,
        BoundaryFindService boundaryFindService) {
        UiApplication = uiApplication;
        _outerSquareService = outerSquareService;
        _elementSectionService = elementSectionService;
        _curveService = curveService;
        _boundaryFindService = boundaryFindService;
    }

    private UIApplication UiApplication { get; }
    private UIDocument ActiveUiDocument => UiApplication.ActiveUIDocument;
    public Application Application => UiApplication.Application;
    public Document Document => ActiveUiDocument.Document;
    

    public void Action() {
        // Получаем кривые с геометрии вида
        var activeView = ActiveUiDocument.ActiveView;
        var curves = _elementSectionService.GetSectionCurves(Document, activeView);

        // Проекция в XY
        var projected = curves.Select(curve => _curveService.ProjectCurveToXy(curve)).ToList();

        // Внешний квадрат
        var outerSquare = _outerSquareService.BuildOuterSquareVertices(projected, _sideCell);

        // Нарезка на короткие куски (Intersect стабильнее)
        double maxLenMm = UnitUtils.ConvertToInternalUnits(200, UnitTypeId.Millimeters);
        var fragmentedCurves = projected.SelectMany(curve => _curveService.SplitToShortCurves(curve, maxLenMm)).ToList();

        // Волна от 4 углов 
        var cellsSquares = _boundaryFindService.GetCellsSquare(outerSquare, fragmentedCurves, _sideCell);
        
        // Записываем в клетку все ее кривые
        foreach(var cell in cellsSquares) {
            var polygon = new List<XYZ> {
                cell.BottomLeft,
                cell.TopLeft,
                cell.BottomRight,
                cell.TopRight
            };
            
            var curvesInside = fragmentedCurves
                .Where(x => IsCurveInsideCell(x, cell))
                .ToList();
            
            cell.Curves = curvesInside;
        }

        foreach(var cell in cellsSquares) {
            var outPoint = GetOutsidePoint(cell);
            var curvess = _boundaryFindService.GetBoundaryCurves(cell, outPoint, cell.Curves, UnitUtils.ConvertToInternalUnits(10, UnitTypeId.Millimeters));
            foreach(var curve in curvess) {
                Document.Create.NewDetailCurve(activeView, curve);
            }
            
        }


        // double maxLenCurveMm = UnitUtils.ConvertToInternalUnits(10, UnitTypeId.Millimeters);
        // foreach(var cell in cellsSquares) {
        //     
        //     // double rayLength = cell.BottomLeft.DistanceTo(cell.TopRight) * 2;
        //     // var contour = FindVisibleBoundaryCurves(
        //     //     GetOutsidePoint(cell),
        //     //     cell.Curves,
        //     //     rayLength,
        //     //     1.0);
        //     
        //     var fragmentedCurv = cell.Curves.SelectMany(curve => _curveService.SplitToShortCurves(curve, maxLenCurveMm)).ToList();
        //     var contour = _boundaryFindService.FindBoundaryCurves(cell, GetOutsidePoint(cell), fragmentedCurv, 10);
        //     
        //     foreach(var curve in contour) {
        //         Document.Create.NewDetailCurve(activeView, curve);
        //     }
        //     
        // }



        // // Рандомная клетка
        // var randomCell = cellsSquares.ToList()[214];
        //
        // // Рисуем контур рандомной клетки
        // DrawCell(activeView, randomCell);
        //
        // // Точка
        // var stPoint = GetOutsidePoint(randomCell);
        // var circle = Arc.Create(stPoint, 0.2, 0, 2 * Math.PI, XYZ.BasisX, XYZ.BasisY);
        // Document.Create.NewDetailCurve(activeView, circle);
        //
        // var ptt = new List<XYZ> { randomCell.TopLeft, randomCell.TopRight, randomCell.BottomRight, randomCell.BottomLeft };
        //
        // var outPoint = GetOutsidePoint(randomCell);
        //
        // var cells = _boundaryFindService.GetCellsSquare(randomCell, outPoint, randomCell.Curves, UnitUtils.ConvertToInternalUnits(10, UnitTypeId.Millimeters));
        //
        // System.Windows.MessageBox.Show(cells.Count.ToString());
        //
        // foreach(var cell in cells) {
        //     DrawCell(activeView, cell);
        // }
        
        // // Нарисовать все кривые рандомной клетки
        // foreach(var curve in randomCell.Curves) {
        //     Document.Create.NewDetailCurve(activeView, curve);
        // }
        


        // // Отрисовка клеток
        // var pts = new List<XYZ>();
        // foreach(var cell in cellsSquares) {
        //     if(cell.BLType == CellVertexType.Outside) {
        //         pts.Add(cell.BottomLeft);
        //     }
        //     if(cell.TLType == CellVertexType.Outside) {
        //         pts.Add(cell.TopLeft);
        //     }
        //     if(cell.BRType == CellVertexType.Outside) {
        //         pts.Add(cell.BottomRight);
        //     }
        //     if(cell.TRType == CellVertexType.Outside) {
        //         pts.Add(cell.TopRight);
        //     }
        //     DrawCell(activeView, cell);
        // }
        //
        // foreach(var pt in pts) {
        //     var circle = Arc.Create(pt, 0.3, 0, 2 * Math.PI, XYZ.BasisX, XYZ.BasisY);
        //     Document.Create.NewDetailCurve(activeView, circle);
        // }
        
        // foreach(var curve in fragmentedCurves) {
        //     Document.Create.NewDetailCurve(activeView, curve);
        // }
    }
    
    public static HashSet<Curve> FindVisibleBoundaryCurves(
        XYZ startPoint,
        List<Curve> curves,
        double rayLength,
        double angleStepDeg = 1.0)
    {
        var result = new HashSet<Curve>();

        for(double angle = 0; angle < 360.0; angle += angleStepDeg) {
            double rad = angle * Math.PI / 180.0;

            var dir = new XYZ(
                Math.Cos(rad),
                Math.Sin(rad),
                0);

            var rayEnd = startPoint + dir.Multiply(rayLength);

            var ray = Line.CreateBound(startPoint, rayEnd);

            Curve nearestCurve = null;
            double nearestDistance = double.MaxValue;

            foreach(var curve in curves) {
                var cmp = ray.Intersect(curve, out IntersectionResultArray ira);

                if(cmp != SetComparisonResult.Overlap ||
                   ira == null ||
                   ira.Size == 0) {
                    continue;
                }

                for(int i = 0; i < ira.Size; i++) {
                    var point = ira.get_Item(i)?.XYZPoint;

                    if(point == null) {
                        continue;
                    }

                    double dist = startPoint.DistanceTo(point);

                    if(dist < nearestDistance) {
                        nearestDistance = dist;
                        nearestCurve = curve;
                    }
                }
            }

            if(nearestCurve != null) {
                result.Add(nearestCurve);
            }
        }

        return result;
    }
    
    public static List<Curve> GetNearestContour(
    CellSquare cell,
    double joinTolerance)
{
    XYZ outsidePoint = GetOutsidePoint(cell);

    if(outsidePoint == null || cell.Curves == null || cell.Curves.Count == 0)
        return new List<Curve>();

    // Разбиваем все линии на связанные группы
    var groups = BuildCurveGroups(cell.Curves, joinTolerance);

    // Ищем группу с минимальным расстоянием до внешней точки
    List<Curve> bestGroup = null;
    double bestDistance = double.MaxValue;

    foreach(var group in groups) {
        double groupDistance = group.Min(x => x.Distance(outsidePoint));

        if(groupDistance < bestDistance) {
            bestDistance = groupDistance;
            bestGroup = group;
        }
    }

    return bestGroup ?? new List<Curve>();
}


    private static List<List<Curve>> BuildCurveGroups(List<Curve> curves, double tolerance) {
        var result = new List<List<Curve>>();
        var visited = new HashSet<int>();

        for(int i = 0; i < curves.Count; i++) {
            if(visited.Contains(i))
                continue;

            var group = new List<Curve>();
            var queue = new Queue<int>();

            queue.Enqueue(i);
            visited.Add(i);

            while(queue.Count > 0) {
                int current = queue.Dequeue();

                Curve currentCurve = curves[current];
                group.Add(currentCurve);

                for(int j = 0; j < curves.Count; j++) {
                    if(visited.Contains(j))
                        continue;

                    if(IsConnected(
                            currentCurve,
                            curves[j],
                            tolerance)) {

                        visited.Add(j);
                        queue.Enqueue(j);
                    }
                }
            }

            result.Add(group);
        }

        return result;
    }

    private static bool IsConnected(Curve a, Curve b, double tolerance) {
        XYZ a0 = a.GetEndPoint(0);
        XYZ a1 = a.GetEndPoint(1);

        XYZ b0 = b.GetEndPoint(0);
        XYZ b1 = b.GetEndPoint(1);

        return a0.DistanceTo(b0) <= tolerance
               || a0.DistanceTo(b1) <= tolerance
               || a1.DistanceTo(b0) <= tolerance
               || a1.DistanceTo(b1) <= tolerance;
    }
    
    
    
    private static XYZ GetOutsidePoint(CellSquare cell) {
        if(cell.BLType == CellVertexType.Outside)
            return cell.BottomLeft;

        if(cell.BRType == CellVertexType.Outside)
            return cell.BottomRight;

        if(cell.TRType == CellVertexType.Outside)
            return cell.TopRight;

        if(cell.TLType == CellVertexType.Outside)
            return cell.TopLeft;

        return null;
    }
    
    public static bool IsPointInsideCell(XYZ point, CellSquare cell) {
        double minX = Math.Min(cell.BottomLeft.X, cell.BottomRight.X);
        double maxX = Math.Max(cell.TopLeft.X, cell.TopRight.X);

        double minY = Math.Min(cell.BottomLeft.Y, cell.TopLeft.Y);
        double maxY = Math.Max(cell.BottomRight.Y, cell.TopRight.Y);

        return point.X >= minX
               && point.X <= maxX
               && point.Y >= minY
               && point.Y <= maxY;
    }
    
    public static bool IsCurveInsideCell(Curve curve, CellSquare cell) {
        XYZ p1 = curve.GetEndPoint(0);
        XYZ p2 = curve.GetEndPoint(1);
        return IsPointInsideCell(p1, cell) || IsPointInsideCell(p2, cell);
    }

    private void DrawCell(View view, CellSquare cell) {
        var doc = Document;
        doc.Create.NewDetailCurve(view, Line.CreateBound(cell.BottomLeft, cell.BottomRight));
        doc.Create.NewDetailCurve(view, Line.CreateBound(cell.BottomRight, cell.TopRight));
        doc.Create.NewDetailCurve(view, Line.CreateBound(cell.TopRight, cell.TopLeft));
        doc.Create.NewDetailCurve(view, Line.CreateBound(cell.TopLeft, cell.BottomLeft));
    }
}
