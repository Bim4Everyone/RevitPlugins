using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using RevitAreaBoundaries.Models;

namespace RevitAreaBoundaries.Services;

internal class FreeEndsJoinService(SystemPluginConfig config)
{
    private readonly double _tol = config.DefaultTolerance;
    
    public List<Curve> JoinNearestFreeEndsSmartIterative(
        List<Curve> curves,
        double maxJoinDistanceMm,
        int maxIterations = 5,
        int maxPairsPerIteration = 100,
        bool avoidCrossings = true,
        double angleWeight = 0.35)
    {
        if (curves == null || curves.Count == 0)
            return new List<Curve>();

        var current = CloneAll(curves);

        for (int iter = 0; iter < maxIterations; iter++)
        {
            int before = current.Count;

            current = JoinNearestFreeEndsSmart(
                current,
                maxJoinDistanceMm,
                maxPairsPerIteration,
                avoidCrossings,
                angleWeight);

            int added = current.Count - before;
            if (added <= 0)
                break; // стабилизация: больше нечего сшивать
        }

        return current;
    }

    /// <summary>
    /// Соединяет свободные концы попарно:
    /// - по расстоянию,
    /// - с учетом угла продолжения,
    /// - без пересечения существующей геометрии (опционально).
    /// </summary>
    public List<Curve> JoinNearestFreeEndsSmart(
        List<Curve> curves,
        double maxJoinDistanceMm,
        int maxPairsPerRun = int.MaxValue,
        bool avoidCrossings = true,
        double angleWeight = 0.35)
    {
        if (curves == null || curves.Count == 0)
            return new List<Curve>();

        double maxJoin = UnitUtils.ConvertToInternalUnits(maxJoinDistanceMm, UnitTypeId.Millimeters);
        if (maxJoin <= _tol)
            return CloneAll(curves);

        var result = CloneAll(curves);

        double cell = Math.Max(maxJoin, UnitUtils.ConvertToInternalUnits(100, UnitTypeId.Millimeters));
        var index = new CurveSpatialIndex(result, cell);

        var freeEnds = CollectFreeEnds(result, index);
        if (freeEnds.Count < 2)
            return result;

        // Кандидаты пар со score = distance + anglePenalty*angleWeight*maxJoin
        var candidates = BuildPairCandidatesWithScore(freeEnds, maxJoin, angleWeight);
        candidates.Sort((a, b) => a.Score.CompareTo(b.Score));

        var used = new bool[freeEnds.Count];
        int added = 0;

        foreach (var c in candidates)
        {
            if (added >= maxPairsPerRun) break;
            if (used[c.A] || used[c.B]) continue;

            var feA = freeEnds[c.A];
            var feB = freeEnds[c.B];

            XYZ p1 = feA.Point;
            XYZ p2 = feB.Point;

            if (p1.DistanceTo(p2) <= _tol) continue;
            if (HasSameLine(result, p1, p2)) continue;

            var bridge = Line.CreateBound(p1, p2);

            if (avoidCrossings && IntersectsAny(bridge, result, feA.CurveIndex, feB.CurveIndex))
                continue;

            result.Add(bridge);
            used[c.A] = true;
            used[c.B] = true;
            added++;
        }

        return result;
    }

    private List<Curve> CloneAll(List<Curve> curves)
    {
        var res = new List<Curve>(curves.Count);
        foreach (var c in curves) res.Add(c.Clone());
        return res;
    }

    private List<FreeEnd> CollectFreeEnds(List<Curve> curves, CurveSpatialIndex index)
    {
        var free = new List<FreeEnd>(curves.Count);

        for (int i = 0; i < curves.Count; i++)
        {
            if (curves[i] is not Line line) continue;

            XYZ p0 = line.GetEndPoint(0);
            if (!IsEndpointConnectedIndexed(p0, i, curves, index, _tol))
            {
                XYZ dirOut0 = (line.GetEndPoint(0) - line.GetEndPoint(1)).Normalize();
                free.Add(new FreeEnd(i, 0, p0, dirOut0));
            }

            XYZ p1 = line.GetEndPoint(1);
            if (!IsEndpointConnectedIndexed(p1, i, curves, index, _tol))
            {
                XYZ dirOut1 = (line.GetEndPoint(1) - line.GetEndPoint(0)).Normalize();
                free.Add(new FreeEnd(i, 1, p1, dirOut1));
            }
        }

        return free;
    }

    private List<PairCandidate> BuildPairCandidatesWithScore(List<FreeEnd> freeEnds, double maxJoin, double angleWeight)
    {
        var pairs = new List<PairCandidate>(freeEnds.Count * 4);
        var grid = BuildPointGrid(freeEnds, maxJoin);

        for (int a = 0; a < freeEnds.Count; a++)
        {
            var fa = freeEnds[a];

            foreach (int b in QueryNearby(grid, fa.Point, maxJoin))
            {
                if (b <= a) continue;
                var fb = freeEnds[b];

                if (fa.CurveIndex == fb.CurveIndex) continue;

                double d = fa.Point.DistanceTo(fb.Point);
                if (d > maxJoin || d <= _tol) continue;

                // Угол: чем меньше отклонение от "продолжения", тем лучше
                XYZ ab = (fb.Point - fa.Point).Normalize();
                XYZ ba = (fa.Point - fb.Point).Normalize();

                // cos=1 => идеально вперед, cos=-1 => назад
                double cosA = Clamp(fa.OutDirection.DotProduct(ab), -1.0, 1.0);
                double cosB = Clamp(fb.OutDirection.DotProduct(ba), -1.0, 1.0);

                // penalty 0..1
                double anglePenaltyA = (1.0 - cosA) * 0.5;
                double anglePenaltyB = (1.0 - cosB) * 0.5;
                double anglePenalty = (anglePenaltyA + anglePenaltyB) * 0.5;

                double score = d + (anglePenalty * angleWeight * maxJoin);
                pairs.Add(new PairCandidate(a, b, d, score));
            }
        }

        return pairs;
    }

    private bool IsEndpointConnectedIndexed(
        XYZ point, int ownerIndex, List<Curve> curves, CurveSpatialIndex index, double tol)
    {
        var probe = BBox.BBoxFromPointXy(point, tol);

        foreach (int i in index.Query(probe))
        {
            if (i == ownerIndex) continue;
            var c = curves[i];

            if (c.GetEndPoint(0).DistanceTo(point) <= tol) return true;
            if (c.GetEndPoint(1).DistanceTo(point) <= tol) return true;

            var pr = c.Project(point);
            if (pr != null && pr.Distance <= tol) return true;
        }

        return false;
    }

    private bool IntersectsAny(Line bridge, List<Curve> curves, int ownerA, int ownerB)
    {
        // Проверяем пересечение нового моста с существующими кривыми.
        // ownerA/ownerB пропускаем, потому что мост к ним как раз примыкает.
        for (int i = 0; i < curves.Count; i++)
        {
            if (i == ownerA || i == ownerB) continue;

            var c = curves[i];

            try
            {
                bridge.Intersect(c, out IntersectionResultArray ira);
                if (ira == null || ira.Size == 0) continue;

                // Разрешаем касание только в концах моста, всё остальное — пересечение
                for (int k = 0; k < ira.Size; k++)
                {
                    XYZ p = ira.get_Item(k)?.XYZPoint;
                    if (p == null) continue;

                    bool atBridgeEnd =
                        p.DistanceTo(bridge.GetEndPoint(0)) <= _tol ||
                        p.DistanceTo(bridge.GetEndPoint(1)) <= _tol;

                    if (!atBridgeEnd)
                        return true;
                }
            }
            catch
            {
                // если API не смог корректно посчитать, лучше быть консервативным
                return true;
            }
        }

        return false;
    }

    private static bool HasSameLine(List<Curve> curves, XYZ p1, XYZ p2)
    {
        foreach (var c in curves)
        {
            if (c is not Line l) continue;
            XYZ a = l.GetEndPoint(0);
            XYZ b = l.GetEndPoint(1);

            bool same = a.IsAlmostEqualTo(p1) && b.IsAlmostEqualTo(p2);
            bool rev = a.IsAlmostEqualTo(p2) && b.IsAlmostEqualTo(p1);
            if (same || rev) return true;
        }
        return false;
    }

    private Dictionary<long, List<int>> BuildPointGrid(List<FreeEnd> points, double cell)
    {
        var grid = new Dictionary<long, List<int>>();
        for (int i = 0; i < points.Count; i++)
        {
            int cx = ToCell(points[i].Point.X, cell);
            int cy = ToCell(points[i].Point.Y, cell);
            long key = Pack(cx, cy);

            if (!grid.TryGetValue(key, out var list))
            {
                list = new List<int>(8);
                grid[key] = list;
            }
            list.Add(i);
        }
        return grid;
    }

    private IEnumerable<int> QueryNearby(Dictionary<long, List<int>> grid, XYZ p, double radius)
    {
        int cx = ToCell(p.X, radius);
        int cy = ToCell(p.Y, radius);

        for (int dx = -1; dx <= 1; dx++)
        for (int dy = -1; dy <= 1; dy++)
        {
            long key = Pack(cx + dx, cy + dy);
            if (!grid.TryGetValue(key, out var list)) continue;

            for (int k = 0; k < list.Count; k++)
                yield return list[k];
        }
    }

    private static int ToCell(double v, double cell) => (int)Math.Floor(v / cell);
    private static long Pack(int x, int y) => ((long)x << 32) ^ (uint)y;
    private static double Clamp(double v, double lo, double hi) => v < lo ? lo : (v > hi ? hi : v);

    private sealed class FreeEnd
    {
        public int CurveIndex { get; }
        public int EndIndex { get; }
        public XYZ Point { get; }
        public XYZ OutDirection { get; } // направление "наружу" от свободного конца

        public FreeEnd(int curveIndex, int endIndex, XYZ point, XYZ outDirection)
        {
            CurveIndex = curveIndex;
            EndIndex = endIndex;
            Point = point;
            OutDirection = outDirection;
        }
    }

    private readonly struct PairCandidate
    {
        public int A { get; }
        public int B { get; }
        public double Distance { get; }
        public double Score { get; }

        public PairCandidate(int a, int b, double distance, double score)
        {
            A = a;
            B = b;
            Distance = distance;
            Score = score;
        }
    }
}
