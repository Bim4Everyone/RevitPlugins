using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using dosymep.Revit;
using dosymep.Revit.Geometry;

namespace RevitAreaBoundaries.Models;

internal class RevitRepository {
    private static readonly ICollection<BuiltInCategory> _allCategories = new BuiltInCategory[] {
        BuiltInCategory.OST_Doors,
        BuiltInCategory.OST_Floors,
        BuiltInCategory.OST_Railings,
        BuiltInCategory.OST_RailingSystem,
        BuiltInCategory.OST_StairsRailing,
        BuiltInCategory.OST_StairsRailingBaluster,
        BuiltInCategory.OST_StairsRailingRail,
        BuiltInCategory.OST_RailingBalusterRail,
        BuiltInCategory.OST_RailingBalusterRailCut,
        BuiltInCategory.OST_RailingHandRail,
        BuiltInCategory.OST_RailingHandRailAboveCut,
        BuiltInCategory.OST_Roofs,
        BuiltInCategory.OST_Walls,
        BuiltInCategory.OST_Windows,
        BuiltInCategory.OST_GenericModel
    };

    public RevitRepository(UIApplication uiApplication) {
        UiApplication = uiApplication;
    }

    private UIApplication UiApplication { get; }
    private UIDocument ActiveUiDocument => UiApplication.ActiveUIDocument;
    public Application Application => UiApplication.Application;
    public Document Document => ActiveUiDocument.Document;

    private bool ElementMatchesCategory(Element element, HashSet<BuiltInCategory> categories) {
        var category = element.Category?.GetBuiltInCategory();
        return category != null && categories.Contains(category.Value);
    }

    public void Action() {
        var activeView = ActiveUiDocument.ActiveView;

        var categorySet = new HashSet<BuiltInCategory>(_allCategories);

        var elements = new FilteredElementCollector(Document, activeView.Id)
            .WhereElementIsNotElementType()
            .Where(element => ElementMatchesCategory(element, categorySet));

        var level = activeView.GenLevel;
        double elevation = level.Elevation;

        // Уровень +1200 мм
        double finalElevation = elevation + (1200 / 304.8);
        var origin = new XYZ(0, 0, finalElevation);
        var positivePlane = Plane.CreateByNormalAndOrigin(new XYZ(0, 0, 1), origin);

        // Доп. срез +500 мм
        var negativeOrigin = new XYZ(0, 0, finalElevation + (500 / 304.8));
        var negativePlane = Plane.CreateByNormalAndOrigin(new XYZ(0, 0, -1), negativeOrigin);

        var curves = new List<Curve>();
        foreach(var element in elements) {
            var solid = GetUnitedSolid(element);
            var resCurves = GetCurvesFromSolid(solid, positivePlane, negativePlane, activeView);
            curves.AddRange(resCurves);
        }

        // Проекция в XY
        var projected = curves.Select(ProjectCurveToXY).ToList();

        // Внешний квадрат
        var outerSquare = BuildOuterSquareVerticesX2(projected, 1000);

        // Нарезка на короткие куски (Intersect стабильнее)
        double maxLen = UnitUtils.ConvertToInternalUnits(200, UnitTypeId.Millimeters);
        var super = new List<Curve>();
        foreach(var curve in projected) {
            super.AddRange(CurveSplitUtils.SplitToShortCurves(curve, maxLen));
        }

        // Волна от 4 углов 
        var hitCells = GrowWaveFastCoarse4Starts(outerSquare, super, 1000);

        // Отрисовка клеток
        foreach(var cell in hitCells) {
            DrawCell(activeView, cell);
        }

        // foreach(var curve in projected) {
        //     Document.Create.NewDetailCurve(activeView, curve);
        // }
    }

    // ========================== WAVE (4 starts, round-robin) ==========================
    

    public static HashSet<CellSquare> GrowWaveFastCoarse4Starts(
        List<XYZ> squareVertices,
        List<Curve> curves,
        double stepMm)
    {
        double step = UnitUtils.ConvertToInternalUnits(stepMm, UnitTypeId.Millimeters);

        // eps лучше держать небольшим, чтобы не было ложных попаданий
        double eps = UnitUtils.ConvertToInternalUnits(0, UnitTypeId.Millimeters);

        double minX = squareVertices.Min(p => p.X);
        double minY = squareVertices.Min(p => p.Y);
        double maxX = squareVertices.Max(p => p.X);
        double maxY = squareVertices.Max(p => p.Y);

        // 4 угла, выравниваем к сетке
        XYZ bl = AlignToGridFloor(new XYZ(minX, minY, 0), step);
        XYZ br = AlignToGridFloor(new XYZ(maxX, minY, 0), step);
        XYZ tr = AlignToGridFloor(new XYZ(maxX, maxY, 0), step);
        XYZ tl = AlignToGridFloor(new XYZ(minX, maxY, 0), step);

        var index = new CurveSpatialIndex(curves, cellSize: step);

        // 4 очереди (round-robin)
        var queues = new[] {
            new Queue<XYZ>(),
            new Queue<XYZ>(),
            new Queue<XYZ>(),
            new Queue<XYZ>()
        };

        // базовые 4 угла
        queues[0].Enqueue(bl);
        queues[1].Enqueue(br);
        queues[2].Enqueue(tr);
        queues[3].Enqueue(tl);

        // + стартовые точки по периметру (даёт доступ во "внутренние углы/карманы")
        foreach(var p in GetPerimeterSeeds(minX, minY, maxX, maxY, step)) {
            var ap = AlignToGridFloor(p, step);

            // разложим по очередям round-robin, чтобы нагрузка была равномерной
            // (можно и просто в одну очередь, но так лучше)
            int bucket = Math.Abs(GetKey(ap, step).GetHashCode()) % 4;
            queues[bucket].Enqueue(ap);
        }

        var visited = new HashSet<string>();
        var hitCells = new HashSet<CellSquare>();

        bool AnyQueueNotEmpty() =>
            queues[0].Count > 0 || queues[1].Count > 0 || queues[2].Count > 0 || queues[3].Count > 0;

        int qi = 0;

        while(AnyQueueNotEmpty()) {
            // найти непустую очередь
            int tries = 0;
            while(tries < 4 && queues[qi].Count == 0) {
                qi = (qi + 1) % 4;
                tries++;
            }
            if(tries == 4)
                break;

            int source = qi;
            XYZ current = queues[source].Dequeue();
            qi = (source + 1) % 4;

            string currentKey = GetKey(current, step);
            if(!visited.Add(currentKey))
                continue;

            foreach(XYZ next in GetNeighbours(current, step)) {
                if(next.X < minX || next.X > maxX || next.Y < minY || next.Y > maxY)
                    continue;

                string nextKey = GetKey(next, step);
                if(visited.Contains(nextKey))
                    continue;

                bool hit = FindHitFast(current, next, index, eps, out XYZ hitPoint);
                if(hit) {
                    // фиксируем клетку по ТОЧКЕ пересечения (убирает смещения)
                    if(hitPoint != null) {
                        hitCells.Add(MakeCellFromPoint(hitPoint, step));
                    } else {
                        // fallback для касаний/почти касаний
                        hitCells.Add(MakeCellFromEdge(current, next, step));
                    }
                    continue;
                }

                queues[source].Enqueue(next);
            }
        }

        return hitCells;
    }
    
    private static IEnumerable<XYZ> GetPerimeterSeeds(double minX, double minY, double maxX, double maxY, double step) {
        // идём по сетке по периметру
        for(double x = minX; x <= maxX; x += step) {
            yield return new XYZ(x, minY, 0);
            yield return new XYZ(x, maxY, 0);
        }
        for(double y = minY; y <= maxY; y += step) {
            yield return new XYZ(minX, y, 0);
            yield return new XYZ(maxX, y, 0);
        }
    }

    private static IEnumerable<XYZ> GetNeighbours(XYZ point, double step) {
        yield return new XYZ(point.X + step, point.Y, 0);
        yield return new XYZ(point.X - step, point.Y, 0);
        yield return new XYZ(point.X, point.Y + step, 0);
        yield return new XYZ(point.X, point.Y - step, 0);

        yield return new XYZ(point.X + step, point.Y + step, 0);
        yield return new XYZ(point.X + step, point.Y - step, 0);
        yield return new XYZ(point.X - step, point.Y + step, 0);
        yield return new XYZ(point.X - step, point.Y - step, 0);
    }

    private static XYZ AlignToGridFloor(XYZ p, double step) {
        int ix = (int)Math.Floor(p.X / step);
        int iy = (int)Math.Floor(p.Y / step);
        return new XYZ(ix * step, iy * step, 0);
    }

    /// <summary>
    /// Ищем пересечение ребра (from->to) с кривыми.
    /// Если есть реальная точка пересечения -> возвращаем hitPoint.
    /// Если только касание (через Distance eps) -> hitPoint=null, но hit=true.
    /// </summary>
    private static bool FindHitFast(
        XYZ from,
        XYZ to,
        CurveSpatialIndex index,
        double eps,
        out XYZ hitPoint)
    {
        hitPoint = null;

        // Отрезок перехода (ребро сетки)
        Line probe = Line.CreateBound(from, to);

        // bbox отрезка в XY + расширение на eps (вместо Inflate)
        BBox2 probeBox0 = BBox2.FromSegmentXY(from, to);
        BBox2 probeBox = new BBox2(
            probeBox0.MinX - eps,
            probeBox0.MinY - eps,
            probeBox0.MaxX + eps,
            probeBox0.MaxY + eps);

        foreach(int id in index.Query(probeBox)) {
            // дополнительная защита: индекс мог вернуть лишнее
            if(!probeBox.Intersects(index.Boxes[id]))
                continue;

            Curve c = index.Curves[id];

            // 1) Пытаемся получить реальную точку пересечения
            if(TryGetIntersectionPoint(probe, c, out XYZ ip)) {
                hitPoint = new XYZ(ip.X, ip.Y, 0);
                return true;
            }

            // 2) Если Intersect "молчит" — проверяем блокировку по расстоянию
            //    (устойчивее, чем проверка только концов/мидпоинта)
            if(CurveBlocksSegmentByDistance(from, to, c, eps, out XYZ nearPoint)) {
                hitPoint = nearPoint; // точка на сегменте (приблизительно)
                return true;
            }

            // 3) Фолбэк (если хочешь оставить твой старый метод касания)
            // if(SegmentBlocks(probe, c, eps)) return true;
        }

        return false;
    }
    
    private static bool CurveBlocksSegmentByDistance(
    XYZ segA,
    XYZ segB,
    Curve boundary,
    double eps,
    out XYZ nearPointOnSegment)
{
    nearPointOnSegment = null;

    // работаем в 2D
    XYZ a = new XYZ(segA.X, segA.Y, 0);
    XYZ b = new XYZ(segB.X, segB.Y, 0);

    // Самый частый случай: boundary - Line (после нарезки)
    if(boundary is Line bl) {
        XYZ c = bl.GetEndPoint(0); c = new XYZ(c.X, c.Y, 0);
        XYZ d = bl.GetEndPoint(1); d = new XYZ(d.X, d.Y, 0);

        double dist = SegmentSegmentDistance2D(a, b, c, d, out XYZ closestOnAB);
        if(dist <= eps) {
            nearPointOnSegment = closestOnAB;
            return true;
        }

        return false;
    }

    // Fallback для дуг/сплайнов: берём несколько точек на boundary и меряем дистанцию до отрезка
    // (можно увеличить samples, если дуг много)
    const int samples = 10;
    double best = double.MaxValue;
    XYZ bestP = null;

    IList<XYZ> tess = boundary.Tessellate();
    if(tess != null && tess.Count >= 2) {
        // По тесселяции тоже можно пройти сегментами — точнее
        for(int i = 0; i < tess.Count - 1; i++) {
            XYZ c = new XYZ(tess[i].X, tess[i].Y, 0);
            XYZ d = new XYZ(tess[i+1].X, tess[i+1].Y, 0);
            double dist = SegmentSegmentDistance2D(a, b, c, d, out XYZ closestOnAB);
            if(dist < best) { best = dist; bestP = closestOnAB; }
            if(best <= eps) break;
        }
    } else {
        // совсем плохой случай: семплируем параметрически
        double p0 = boundary.GetEndParameter(0);
        double p1 = boundary.GetEndParameter(1);

        for(int i = 0; i <= samples; i++) {
            double t = (double)i / samples;
            double p = p0 + (p1 - p0) * t;
            XYZ q = boundary.Evaluate(p, false);
            q = new XYZ(q.X, q.Y, 0);

            double dist = PointSegmentDistance2D(q, a, b, out XYZ closestOnAB);
            if(dist < best) { best = dist; bestP = closestOnAB; }
            if(best <= eps) break;
        }
    }

    if(best <= eps) {
        nearPointOnSegment = bestP ?? a;
        return true;
    }

    return false;
}
    
    private static double PointSegmentDistance2D(XYZ p, XYZ a, XYZ b, out XYZ closest)
{
    XYZ ab = b - a;
    double ab2 = ab.X * ab.X + ab.Y * ab.Y;

    if(ab2 < 1e-12) {
        closest = a;
        return Math.Sqrt((p.X - a.X) * (p.X - a.X) + (p.Y - a.Y) * (p.Y - a.Y));
    }

    double t = ((p.X - a.X) * ab.X + (p.Y - a.Y) * ab.Y) / ab2;
    t = Math.Max(0.0, Math.Min(1.0, t));

    closest = new XYZ(a.X + ab.X * t, a.Y + ab.Y * t, 0);
    double dx = p.X - closest.X;
    double dy = p.Y - closest.Y;
    return Math.Sqrt(dx * dx + dy * dy);
}

private static double SegmentSegmentDistance2D(
    XYZ a, XYZ b,
    XYZ c, XYZ d,
    out XYZ closestOnAB)
{
    // если пересекаются — дистанция 0
    if(SegmentsIntersect2D(a, b, c, d, out XYZ ip)) {
        closestOnAB = ip; // точка пересечения как closest
        return 0.0;
    }

    // иначе минимум из дистанций концов к противоположным отрезкам
    double best = double.MaxValue;
    closestOnAB = a;

    double dist;

    dist = PointSegmentDistance2D(a, c, d, out _);
    if(dist < best) { best = dist; closestOnAB = a; }

    dist = PointSegmentDistance2D(b, c, d, out _);
    if(dist < best) { best = dist; closestOnAB = b; }

    dist = PointSegmentDistance2D(c, a, b, out XYZ ca);
    if(dist < best) { best = dist; closestOnAB = ca; }

    dist = PointSegmentDistance2D(d, a, b, out XYZ da);
    if(dist < best) { best = dist; closestOnAB = da; }

    return best;
}

private static bool SegmentsIntersect2D(XYZ a, XYZ b, XYZ c, XYZ d, out XYZ ip)
{
    ip = null;

    double ax = a.X, ay = a.Y;
    double bx = b.X, by = b.Y;
    double cx = c.X, cy = c.Y;
    double dx = d.X, dy = d.Y;

    double rpx = bx - ax, rpy = by - ay;
    double spx = dx - cx, spy = dy - cy;

    double rxs = rpx * spy - rpy * spx;
    double qpx = cx - ax, qpy = cy - ay;
    double qpxr = qpx * rpy - qpy * rpx;

    const double tol = 1e-12;

    if(Math.Abs(rxs) < tol && Math.Abs(qpxr) < tol) {
        // коллинеарны — считаем "есть пересечение" только если проекции пересекаются
        // для нашей задачи это можно трактовать как пересечение
        if(CollinearOverlap1D(ax, bx, cx, dx) && CollinearOverlap1D(ay, by, cy, dy)) {
            // точку можно взять любую на пересечении
            ip = a;
            return true;
        }
        return false;
    }

    if(Math.Abs(rxs) < tol && Math.Abs(qpxr) >= tol)
        return false; // параллельны и не коллинеарны

    double t = (qpx * spy - qpy * spx) / rxs;
    double u = (qpx * rpy - qpy * rpx) / rxs;

    if(t >= -1e-9 && t <= 1 + 1e-9 && u >= -1e-9 && u <= 1 + 1e-9) {
        ip = new XYZ(ax + t * rpx, ay + t * rpy, 0);
        return true;
    }

    return false;
}

private static bool CollinearOverlap1D(double a0, double a1, double b0, double b1)
{
    double minA = Math.Min(a0, a1), maxA = Math.Max(a0, a1);
    double minB = Math.Min(b0, b1), maxB = Math.Max(b0, b1);
    return maxA >= minB && maxB >= minA;
}
    

    private static bool TryGetIntersectionPoint(Line probe, Curve boundary, out XYZ ip) {
        ip = null;

        var res = probe.Intersect(boundary, out IntersectionResultArray ira);
        if(res != SetComparisonResult.Overlap || ira == null || ira.Size == 0)
            return false;

        var r = ira.get_Item(0);
        ip = r?.XYZPoint;
        return ip != null;
    }

    private static bool SegmentBlocks(Curve probe, Curve boundary, double eps) {
        // В Revit API:
        // Disjoint  -> нет пересечений
        // Всё остальное (Overlap, Subset, Superset, Equal) трактуем как блокировку
        // (то есть если кривые хоть как-то пересекаются/накладываются — считаем препятствием)
        var cmp = probe.Intersect(boundary);
        if(cmp != SetComparisonResult.Disjoint)
            return true;

        // Фолбэк на случай численных погрешностей: "почти касание"
        XYZ a = probe.GetEndPoint(0);
        XYZ b = probe.GetEndPoint(1);
        XYZ mid = new XYZ((a.X + b.X) * 0.5, (a.Y + b.Y) * 0.5, (a.Z + b.Z) * 0.5);

        if(boundary.Distance(a) < eps) return true;
        if(boundary.Distance(mid) < eps) return true;
        return boundary.Distance(b) < eps;
    }

    private static CellSquare MakeCellFromPoint(XYZ p, double step) {
        int ix = (int)Math.Floor(p.X / step);
        int iy = (int)Math.Floor(p.Y / step);

        XYZ bl = new XYZ(ix * step, iy * step, 0);
        XYZ br = new XYZ((ix + 1) * step, iy * step, 0);
        XYZ tr = new XYZ((ix + 1) * step, (iy + 1) * step, 0);
        XYZ tl = new XYZ(ix * step, (iy + 1) * step, 0);

        return new CellSquare {
            BottomLeft = bl,
            BottomRight = br,
            TopRight = tr,
            TopLeft = tl,
            BLType = CellVertexType.Boundary,
            BRType = CellVertexType.Boundary,
            TRType = CellVertexType.Boundary,
            TLType = CellVertexType.Boundary
        };
    }

    private static CellSquare MakeCellFromEdge(XYZ from, XYZ to, double step) {
        double x0 = Math.Min(from.X, to.X);
        double y0 = Math.Min(from.Y, to.Y);

        int ix = (int)Math.Floor(x0 / step);
        int iy = (int)Math.Floor(y0 / step);

        XYZ bl = new XYZ(ix * step, iy * step, 0);
        XYZ br = new XYZ((ix + 1) * step, iy * step, 0);
        XYZ tr = new XYZ((ix + 1) * step, (iy + 1) * step, 0);
        XYZ tl = new XYZ(ix * step, (iy + 1) * step, 0);

        return new CellSquare {
            BottomLeft = bl,
            BottomRight = br,
            TopRight = tr,
            TopLeft = tl,
            BLType = CellVertexType.Boundary,
            BRType = CellVertexType.Boundary,
            TRType = CellVertexType.Boundary,
            TLType = CellVertexType.Boundary
        };
    }

    // Ключ через Floor, а не Round (меньше дрожания)
    private static string GetKey(XYZ point, double step) {
        int x = (int)Math.Floor(point.X / step);
        int y = (int)Math.Floor(point.Y / step);
        return $"{x}_{y}";
    }

    // ========================== DRAW ==========================

    private void DrawCell(View view, CellSquare cell) {
        var doc = Document;

        doc.Create.NewDetailCurve(view, Line.CreateBound(cell.BottomLeft, cell.BottomRight));
        doc.Create.NewDetailCurve(view, Line.CreateBound(cell.BottomRight, cell.TopRight));
        doc.Create.NewDetailCurve(view, Line.CreateBound(cell.TopRight, cell.TopLeft));
        doc.Create.NewDetailCurve(view, Line.CreateBound(cell.TopLeft, cell.BottomLeft));
    }

    // ========================== GEOMETRY (your original) ==========================

    private static Curve ProjectCurveToXY(Curve curve) {
        XYZ p1 = curve.GetEndPoint(0);
        XYZ p2 = curve.GetEndPoint(1);
        return Line.CreateBound(
            new XYZ(p1.X, p1.Y, 0),
            new XYZ(p2.X, p2.Y, 0));
    }

    public static List<XYZ> BuildOuterSquareVertices(IEnumerable<Curve> curves, double sideSquare) {
        double minX = double.MaxValue;
        double minY = double.MaxValue;
        double maxX = double.MinValue;
        double maxY = double.MinValue;

        foreach(Curve curve in curves) {
            foreach(XYZ point in curve.Tessellate()) {
                minX = Math.Min(minX, point.X);
                minY = Math.Min(minY, point.Y);
                maxX = Math.Max(maxX, point.X);
                maxY = Math.Max(maxY, point.Y);
            }
        }

        double margin = UnitUtils.ConvertToInternalUnits(sideSquare, UnitTypeId.Millimeters);
        minX -= margin;
        minY -= margin;
        maxX += margin;
        maxY += margin;

        double width = maxX - minX;
        double height = maxY - minY;

        double side = Math.Max(width, height);
        double gridSize = UnitUtils.ConvertToInternalUnits(sideSquare, UnitTypeId.Millimeters);
        side = Math.Ceiling(side / gridSize) * gridSize;

        double centerX = (minX + maxX) * 0.5;
        double centerY = (minY + maxY) * 0.5;
        double halfSide = side * 0.5;

        double squareMinX = centerX - halfSide;
        double squareMinY = centerY - halfSide;
        double squareMaxX = centerX + halfSide;
        double squareMaxY = centerY + halfSide;

        return new List<XYZ> {
            new XYZ(squareMinX, squareMinY, 0),
            new XYZ(squareMaxX, squareMinY, 0),
            new XYZ(squareMaxX, squareMaxY, 0),
            new XYZ(squareMinX, squareMaxY, 0)
        };
    }
    
    public static List<XYZ> BuildOuterSquareVerticesX2(IEnumerable<Curve> curves, double gridStepMm) {
        // Сначала строим обычный квадрат как у тебя
        var square = BuildOuterSquareVertices(curves, gridStepMm);

        double minX = square.Min(p => p.X);
        double minY = square.Min(p => p.Y);
        double maxX = square.Max(p => p.X);
        double maxY = square.Max(p => p.Y);

        double cx = (minX + maxX) * 0.5;
        double cy = (minY + maxY) * 0.5;

        double side = Math.Max(maxX - minX, maxY - minY);
        double half = side * 0.5;

        // Увеличиваем сторону в 2 раза => half тоже в 2 раза
        double half2 = half * 2.0;

        return new List<XYZ> {
            new XYZ(cx - half2, cy - half2, 0),
            new XYZ(cx + half2, cy - half2, 0),
            new XYZ(cx + half2, cy + half2, 0),
            new XYZ(cx - half2, cy + half2, 0),
        };
    }

    private List<Curve> GetCurvesFromSolid(Solid solid, Plane positivePlane, Plane negativePlane, View activeView) {
        var curves = new List<Curve>();
        try {
            if(solid == null) return curves;

            var resultSolid = BooleanOperationsUtils.CutWithHalfSpace(solid, positivePlane);
            if(resultSolid == null) return curves;

            var finalSolid = BooleanOperationsUtils.CutWithHalfSpace(resultSolid, negativePlane);
            if(finalSolid == null) return curves;

            var downFaces = new List<Face>();
            foreach(Face face in finalSolid.Faces) {
                if(face is not PlanarFace planarFace) continue;
                if(planarFace.FaceNormal.Z == -1) downFaces.Add(planarFace);
            }

            foreach(var face in downFaces) {
                foreach(EdgeArray loop in face.EdgeLoops) {
                    foreach(Edge edge in loop) {
                        curves.Add(edge.AsCurve());
                    }
                }
            }
        } catch {
            // ignored
        }
        return curves;
    }

    private Solid GetUnitedSolid(Element element) {
        var solids = element.GetSolids().ToArray();
        if(!solids.Any())
            return null;

        var unitedSolids = SolidExtensions.CreateUnitedSolids(solids);

        var validSolids = unitedSolids
            .Where(s => s != null && s.Faces.Size > 0 && s.Edges.Size > 0)
            .ToList();

        return validSolids
            .OrderByDescending(GetSafeSolidVolume)
            .FirstOrDefault();
    }

    private double GetSafeSolidVolume(Solid solid) {
        if(solid == null) return 0;
        try {
            return solid.Volume;
        } catch {
            return 0;
        }
    }
}
