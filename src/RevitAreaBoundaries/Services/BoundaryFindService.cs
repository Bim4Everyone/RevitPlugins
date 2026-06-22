using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitAreaBoundaries.Models;

namespace RevitAreaBoundaries.Services;

public class BoundaryFindService {
    
    public HashSet<Curve> GetBoundaryCurves(
        CellSquare cell,
        XYZ startPoint,
        List<Curve> curves,
        double step)
    {
        double eps = UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Millimeters);

        double minX = cell.BottomLeft.X;
        double minY = cell.BottomLeft.Y;

        double maxX = cell.TopRight.X;
        double maxY = cell.TopRight.Y;

        var start = AlignToGridFloor(startPoint, step);

        double nudge = step * 0.01;
        start = new XYZ(start.X + nudge, start.Y + nudge, 0);

        var index = new CurveSpatialIndex(curves, step);

        var queue = new Queue<XYZ>();
        queue.Enqueue(start);

        var visited = new HashSet<string>();
        var hitCurves = new HashSet<Curve>();

        while (queue.Count > 0)
        {
            XYZ current = queue.Dequeue();

            string currentKey = GetKey(current, step);

            if (!visited.Add(currentKey))
                continue;

            foreach (var next in GetNeighbours(current, step))
            {
                if (next.X < minX ||
                    next.X > maxX ||
                    next.Y < minY ||
                    next.Y > maxY)
                {
                    continue;
                }

                string nextKey = GetKey(next, step);

                if (visited.Contains(nextKey))
                    continue;

                if (FindNearestHitCurveFast(
                        current,
                        next,
                        index,
                        eps,
                        out Curve hitCurve))
                {
                    hitCurves.Add(hitCurve);
                    continue;
                }

                queue.Enqueue(next);
            }
        }

        return hitCurves;
    }
    
    private static bool FindNearestHitCurveFast(
        XYZ from,
        XYZ to,
        CurveSpatialIndex index,
        double eps,
        out Curve hitCurve)
    {
        hitCurve = null;

        var probe = Line.CreateBound(from, to);

        var probeBox0 = BBox.FromSegmentXY(from, to);
        var probeBox = new BBox(
            probeBox0.MinX - eps,
            probeBox0.MinY - eps,
            probeBox0.MaxX + eps,
            probeBox0.MaxY + eps);

        double bestDistance = double.MaxValue;

        foreach (int id in index.Query(probeBox))
        {
            if (!probeBox.Intersects(index.Boxes[id]))
                continue;

            Curve curve = index.Curves[id];
            XYZ hitPoint = null;

            if (TryGetIntersectionPoint(probe, curve, out XYZ ip))
            {
                hitPoint = ip;
            }
            else if (CurveBlocksSegmentByDistance(from, to, curve, eps, out XYZ nearPoint))
            {
                hitPoint = nearPoint;
            }
            else if (SegmentBlocks(probe, curve, eps))
            {
                var projection = curve.Project(from);
                if (projection != null)
                    hitPoint = projection.XYZPoint;
            }

            if (hitPoint == null)
                continue;

            double distance = from.DistanceTo(hitPoint);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                hitCurve = curve;
            }
        }

        return hitCurve != null;
    }
    
    public HashSet<CellSquare> GetCellsSquare(CellSquare cell, XYZ startPoint, List<Curve> curves, double step) {
        double eps = UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Millimeters);

        double minX = cell.BottomLeft.X;
        double minY = cell.BottomLeft.Y;

        double maxX = cell.TopRight.X;
        double maxY = cell.TopRight.Y;

        var start = AlignToGridFloor(startPoint, step);
        
        // сдвиг на 1% шага внутрь, чтобы уйти с границы
        double nudge = step * 0.01;
        start = new XYZ(start.X + nudge, start.Y + nudge, 0);

        var index = new CurveSpatialIndex(curves, step);

        var queue = new Queue<XYZ>();
        queue.Enqueue(start);

        var visited = new HashSet<string>();

        var hitCells = new HashSet<CellSquare>();
        var hitCellKeys = new HashSet<string>();

        while(queue.Count > 0) {
            XYZ current = queue.Dequeue();

            string currentKey = GetKey(current, step);

            if(!visited.Add(currentKey))
                continue;

            foreach(var next in GetNeighbours(current, step)) {

                if(next.X < minX ||
                   next.X > maxX ||
                   next.Y < minY ||
                   next.Y > maxY) {
                    continue;
                }

                string nextKey = GetKey(next, step);

                if(visited.Contains(nextKey))
                    continue;

                bool hit = FindHitFast(current, next, index, eps, out var hitPoint);

                if(hit) {
                    var hitCell = hitPoint != null
                        ? MakeCellFromPoint(hitPoint, step)
                        : MakeCellFromEdge(current, next, step);

                    MarkCellOrientation(hitCell, current);

                    string cellKey =
                        $"{hitCell.BottomLeft.X:F6}_{hitCell.BottomLeft.Y:F6}";

                    if(hitCellKeys.Add(cellKey)) {
                        hitCells.Add(hitCell);
                    }

                    continue;
                }

                queue.Enqueue(next);
            }
        }

        return hitCells;
    }
    
    public HashSet<CellSquare> GetCellsSquare(List<XYZ> squareVertices, List<Curve> curves, double step) {

        // eps лучше держать небольшим, чтобы не было ложных попаданий
        double eps = UnitUtils.ConvertToInternalUnits(0, UnitTypeId.Millimeters);

        double minX = squareVertices.Min(p => p.X);
        double minY = squareVertices.Min(p => p.Y);
        double maxX = squareVertices.Max(p => p.X);
        double maxY = squareVertices.Max(p => p.Y);

        // 4 угла, выравниваем к сетке
        var bl = AlignToGridFloor(new XYZ(minX, minY, 0), step);
        var br = AlignToGridFloor(new XYZ(maxX, minY, 0), step);
        var tr = AlignToGridFloor(new XYZ(maxX, maxY, 0), step);
        var tl = AlignToGridFloor(new XYZ(minX, maxY, 0), step);

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
            int bucket = Math.Abs(GetKey(ap, step).GetHashCode()) % 4;
            queues[bucket].Enqueue(ap);
        }

        var visited = new HashSet<string>();
        var hitCells = new HashSet<CellSquare>();
        var hitCellKeys = new HashSet<string>();

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
                    var cell = hitPoint != null 
                        ? MakeCellFromPoint(hitPoint, step) 
                        : MakeCellFromEdge(current, next, step);
                    
                    MarkCellOrientation(cell, current);

                    string cellKey = $"{cell.BottomLeft.X:F6}_{cell.BottomLeft.Y:F6}";

                    if(hitCellKeys.Add(cellKey)) {
                        hitCells.Add(cell);
                    }
                    continue;
                }

                queues[source].Enqueue(next);
            }
        }

        return hitCells;
        
        bool AnyQueueNotEmpty() => queues[0].Count > 0 || queues[1].Count > 0 || queues[2].Count > 0 || queues[3].Count > 0;
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
    
    private static IEnumerable<XYZ> GetNeighbours4(XYZ point, double step) {
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
    private static bool FindHitFast(XYZ from, XYZ to, CurveSpatialIndex index, double eps, out XYZ hitPoint) {
        hitPoint = null;

        // Отрезок перехода (ребро сетки)
        var probe = Line.CreateBound(from, to);

        // bbox отрезка в XY + расширение на eps
        var probeBox0 = BBox.FromSegmentXY(from, to);
        var probeBox = new BBox(
            probeBox0.MinX - eps,
            probeBox0.MinY - eps,
            probeBox0.MaxX + eps,
            probeBox0.MaxY + eps);

        foreach(int id in index.Query(probeBox)) {
            // дополнительная защита: индекс мог вернуть лишнее
            if(!probeBox.Intersects(index.Boxes[id]))
                continue;
        
            var c = index.Curves[id];
        
            // 1) Пытаемся получить реальную точку пересечения
            if(TryGetIntersectionPoint(probe, c, out XYZ ip)) {
                hitPoint = new XYZ(ip.X, ip.Y, 0);
                return true;
            }
        
            // 2) Если Intersect "молчит" — проверяем блокировку по расстоянию
            if(CurveBlocksSegmentByDistance(from, to, c, eps, out var nearPoint)) {
                hitPoint = nearPoint; // точка на сегменте (приблизительно)
                return true;
            }
        
            // 3) Фолбэк (если хочешь оставить твой старый метод касания)
            if(SegmentBlocks(probe, c, eps)) {
                return true;
            }
        }
        return false;
    }
    
    private static bool CurveBlocksSegmentByDistance(XYZ segA, XYZ segB, Curve boundary, double eps, out XYZ nearPointOnSegment) {
        nearPointOnSegment = null;
        // работаем в 2D
        var a = new XYZ(segA.X, segA.Y, 0);
        var b = new XYZ(segB.X, segB.Y, 0);

        // Самый частый случай: boundary - Line (после нарезки)
        if(boundary is Line bl) {
            var c = bl.GetEndPoint(0); c = new XYZ(c.X, c.Y, 0);
            var d = bl.GetEndPoint(1); d = new XYZ(d.X, d.Y, 0);

            double dist = SegmentSegmentDistance2D(a, b, c, d, out var closestOnAb);
            if(!(dist <= eps)) {
                return false;
            }
            nearPointOnSegment = closestOnAb;
            return true;
        }

        // Fallback для дуг/сплайнов: берём несколько точек на boundary и меряем дистанцию до отрезка
        // (можно увеличить samples, если дуг много)
        const int samples = 10;
        double best = double.MaxValue;
        XYZ bestP = null;

        var tess = boundary.Tessellate();
        if(tess != null && tess.Count >= 2) {
            // По тесселяции тоже можно пройти сегментами — точнее
            for(int i = 0; i < tess.Count - 1; i++) {
                var c = new XYZ(tess[i].X, tess[i].Y, 0);
                var d = new XYZ(tess[i+1].X, tess[i+1].Y, 0);
                double dist = SegmentSegmentDistance2D(a, b, c, d, out var closestOnAb);
                if(dist < best) { best = dist; bestP = closestOnAb; }
                if(best <= eps) break;
            }
        } else {
            // совсем плохой случай: семплируем параметрически
            double p0 = boundary.GetEndParameter(0);
            double p1 = boundary.GetEndParameter(1);

            for(int i = 0; i <= samples; i++) {
                double t = (double)i / samples;
                double p = p0 + (p1 - p0) * t;
                var q = boundary.Evaluate(p, false);
                q = new XYZ(q.X, q.Y, 0);

                double dist = PointSegmentDistance2D(q, a, b, out var closestOnAb);
                if(dist < best) { best = dist; bestP = closestOnAb; }
                if(best <= eps) break;
            }
        }

        if(!(best <= eps)) {
            return false;
        }

        nearPointOnSegment = bestP ?? a;
        return true;
    }
    
    private static double PointSegmentDistance2D(XYZ p, XYZ a, XYZ b, out XYZ closest) {
        var ab = b - a;
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

    private static double SegmentSegmentDistance2D(XYZ a, XYZ b, XYZ c, XYZ d, out XYZ closestOnAb) {
        // если пересекаются — дистанция 0
        if(SegmentsIntersect2D(a, b, c, d, out var ip)) {
            closestOnAb = ip; // точка пересечения как closest
            return 0.0;
        }

        // иначе минимум из дистанций концов к противоположным отрезкам
        double best = double.MaxValue;
        closestOnAb = a;

        double dist = PointSegmentDistance2D(a, c, d, out _);
        if(dist < best) { best = dist; closestOnAb = a; }

        dist = PointSegmentDistance2D(b, c, d, out _);
        if(dist < best) { best = dist; closestOnAb = b; }

        dist = PointSegmentDistance2D(c, a, b, out XYZ ca);
        if(dist < best) { best = dist; closestOnAb = ca; }

        dist = PointSegmentDistance2D(d, a, b, out XYZ da);
        
        if(!(dist < best)) {
            return best;
        }

        best = dist; closestOnAb = da;

        return best;
    }

    private static bool SegmentsIntersect2D(XYZ a, XYZ b, XYZ c, XYZ d, out XYZ ip) {
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
            if(!CollinearOverlap1D(ax, bx, cx, dx) || !CollinearOverlap1D(ay, by, cy, dy)) {
                return false;
            }

            // точку можно взять любую на пересечении
            ip = a;
            return true;
        }

        if(Math.Abs(rxs) < tol && Math.Abs(qpxr) >= tol)
            return false; // параллельны и не коллинеарны

        double t = (qpx * spy - qpy * spx) / rxs;
        double u = (qpx * rpy - qpy * rpx) / rxs;

        if(!(t >= -1e-9) || !(t <= 1 + 1e-9) || !(u >= -1e-9) || !(u <= 1 + 1e-9)) {
            return false;
        }

        ip = new XYZ(ax + t * rpx, ay + t * rpy, 0);
        return true;

    }

    private static bool CollinearOverlap1D(double a0, double a1, double b0, double b1) {
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
        // Если кривые хоть как-то пересекаются/накладываются — считаем препятствием
        var cmp = probe.Intersect(boundary);
        if(cmp != SetComparisonResult.Disjoint) {
            return true;
        }

        // Фолбэк на случай численных погрешностей: "почти касание"
        var a = probe.GetEndPoint(0);
        var b = probe.GetEndPoint(1);
        var mid = new XYZ((a.X + b.X) * 0.5, (a.Y + b.Y) * 0.5, (a.Z + b.Z) * 0.5);

        if(boundary.Distance(a) < eps) return true;
        if(boundary.Distance(mid) < eps) return true;
        
        return boundary.Distance(b) < eps;
    }

    private static CellSquare MakeCellFromPoint(XYZ p, double step) {
        int ix = (int)Math.Floor(p.X / step);
        int iy = (int)Math.Floor(p.Y / step);

        var bl = new XYZ(ix * step, iy * step, 0);
        var br = new XYZ((ix + 1) * step, iy * step, 0);
        var tr = new XYZ((ix + 1) * step, (iy + 1) * step, 0);
        var tl = new XYZ(ix * step, (iy + 1) * step, 0);

        return new CellSquare {
            BottomLeft = bl,
            BottomRight = br,
            TopRight = tr,
            TopLeft = tl,
        };
    }

    private static CellSquare MakeCellFromEdge(XYZ from, XYZ to, double step) {
        double x0 = Math.Min(from.X, to.X);
        double y0 = Math.Min(from.Y, to.Y);

        int ix = (int)Math.Floor(x0 / step);
        int iy = (int)Math.Floor(y0 / step);

        var bl = new XYZ(ix * step, iy * step, 0);
        var br = new XYZ((ix + 1) * step, iy * step, 0);
        var tr = new XYZ((ix + 1) * step, (iy + 1) * step, 0);
        var tl = new XYZ(ix * step, (iy + 1) * step, 0);

        return new CellSquare {
            BottomLeft = bl,
            BottomRight = br,
            TopRight = tr,
            TopLeft = tl,
        };
    }

    // Ключ через Floor, а не Round (меньше дрожания)
    private static string GetKey(XYZ point, double step) {
        int x = (int)Math.Floor(point.X / step);
        int y = (int)Math.Floor(point.Y / step);
        return $"{x}_{y}";
    }
    
    private static void MarkCellOrientation(
        CellSquare cell,
        XYZ outsidePoint)
    {
        cell.BLType = CellVertexType.Boundary;
        cell.BRType = CellVertexType.Boundary;
        cell.TRType = CellVertexType.Boundary;
        cell.TLType = CellVertexType.Boundary;

        var corners = new Dictionary<XYZ, Action>
        {
            [cell.BottomLeft]  = () => cell.BLType = CellVertexType.Outside,
            [cell.BottomRight] = () => cell.BRType = CellVertexType.Outside,
            [cell.TopRight]    = () => cell.TRType = CellVertexType.Outside,
            [cell.TopLeft]     = () => cell.TLType = CellVertexType.Outside
        };

        XYZ nearest = corners
            .Keys
            .OrderBy(x => x.DistanceTo(outsidePoint))
            .First();

        corners[nearest]();
    }
    
}
