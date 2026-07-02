using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RevitAreaBoundaries.Models;

namespace RevitAreaBoundaries.Services;

internal class CollinearLineMergeService(SystemPluginConfig config)
{
    private readonly double _tol = config.DefaultTolerance;

    public List<Curve> MergeConnectedCollinearLines(List<Curve> curves)
    {
        if (curves == null || curves.Count == 0)
            return new List<Curve>();

        // Работаем только с линиями, остальное возвращаем как есть
        var lines = new List<Line>();
        var nonLines = new List<Curve>();

        foreach (var c in curves)
        {
            if (c is Line l) lines.Add(l);
            else nonLines.Add(c);
        }

        if (lines.Count <= 1)
            return curves.Select(c => c.Clone()).ToList();

        // Индекс для ускорения поиска соседей
        double cell = Math.Max(_tol * 10.0, UnitUtils.ConvertToInternalUnits(100, UnitTypeId.Millimeters));
        var lineCurves = lines.Cast<Curve>().ToList();
        var index = new CurveSpatialIndex(lineCurves, cell);

        int n = lines.Count;
        var visited = new bool[n];
        var result = new List<Curve>(curves.Count);

        for (int i = 0; i < n; i++)
        {
            if (visited[i]) continue;

            // Собираем компоненту "связанных коллинеарных"
            var group = CollectCollinearComponent(i, lines, index, visited);

            if (group.Count == 1)
            {
                result.Add(lines[group[0]].Clone());
                continue;
            }

            // Мерджим группу в 1 линию
            var merged = MergeGroup(lines, group);
            result.Add(merged);
        }

        // Добавляем нелинейные кривые обратно
        result.AddRange(nonLines.Select(c => c.Clone()));
        return result;
    }

    private List<int> CollectCollinearComponent(
        int start,
        List<Line> lines,
        CurveSpatialIndex index,
        bool[] visited)
    {
        var group = new List<int>();
        var q = new Queue<int>();
        q.Enqueue(start);
        visited[start] = true;

        while (q.Count > 0)
        {
            int i = q.Dequeue();
            group.Add(i);

            Line li = lines[i];
            BBox bi = index.Boxes[i];

            // немного расширим bbox для поиска "касающихся" концами
            var probe = Inflate(bi, _tol * 2.0);

            foreach (int j in index.Query(probe))
            {
                if (j == i || visited[j]) continue;

                Line lj = lines[j];
                if (!AreCollinear(li, lj, _tol)) continue;
                if (!AreConnectedOrOverlapping(li, lj, _tol)) continue;

                visited[j] = true;
                q.Enqueue(j);
            }
        }

        return group;
    }

    private Line MergeGroup(List<Line> lines, List<int> idxs)
    {
        // Базовая линия для 1D-проекции
        Line baseLine = lines[idxs[0]];
        XYZ origin = baseLine.GetEndPoint(0);
        XYZ dir = (baseLine.GetEndPoint(1) - baseLine.GetEndPoint(0)).Normalize();

        double minT = double.MaxValue;
        double maxT = double.MinValue;

        foreach (int idx in idxs)
        {
            Line l = lines[idx];
            XYZ p0 = l.GetEndPoint(0);
            XYZ p1 = l.GetEndPoint(1);

            double t0 = dir.DotProduct(p0 - origin);
            double t1 = dir.DotProduct(p1 - origin);

            if (t0 > t1) (t0, t1) = (t1, t0);

            if (t0 < minT) minT = t0;
            if (t1 > maxT) maxT = t1;
        }

        XYZ a = origin + dir * minT;
        XYZ b = origin + dir * maxT;

        if (a.DistanceTo(b) <= _tol)
            return Line.CreateBound(baseLine.GetEndPoint(0), baseLine.GetEndPoint(1));

        return Line.CreateBound(a, b);
    }

    private static BBox Inflate(BBox b, double d)
    {
        return new BBox(b.MinX - d, b.MinY - d, b.MaxX + d, b.MaxY + d);
    }

    private static bool AreCollinear(Line l1, Line l2, double tol)
    {
        XYZ d1 = (l1.GetEndPoint(1) - l1.GetEndPoint(0)).Normalize();
        XYZ d2 = (l2.GetEndPoint(1) - l2.GetEndPoint(0)).Normalize();

        // параллельны?
        if (d1.CrossProduct(d2).GetLength() > tol) return false;

        // лежат на одной бесконечной прямой?
        XYZ v = l2.GetEndPoint(0) - l1.GetEndPoint(0);
        return d1.CrossProduct(v).GetLength() <= tol;
    }

    private static bool AreConnectedOrOverlapping(Line l1, Line l2, double tol)
    {
        XYZ a0 = l1.GetEndPoint(0);
        XYZ a1 = l1.GetEndPoint(1);
        XYZ b0 = l2.GetEndPoint(0);
        XYZ b1 = l2.GetEndPoint(1);

        // Быстрый случай: концы касаются
        if (a0.DistanceTo(b0) <= tol || a0.DistanceTo(b1) <= tol ||
            a1.DistanceTo(b0) <= tol || a1.DistanceTo(b1) <= tol)
            return true;

        // Проверка пересечения проекций на ось l1
        XYZ dir = (a1 - a0).Normalize();
        double A0 = dir.DotProduct(a0);
        double A1 = dir.DotProduct(a1);
        double B0 = dir.DotProduct(b0);
        double B1 = dir.DotProduct(b1);

        if (A0 > A1) (A0, A1) = (A1, A0);
        if (B0 > B1) (B0, B1) = (B1, B0);

        // интервалы пересекаются или почти касаются
        return !(A1 < B0 - tol || B1 < A0 - tol);
    }
}
