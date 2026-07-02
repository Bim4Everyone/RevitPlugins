using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using RevitAreaBoundaries.Models;

namespace RevitAreaBoundaries.Services;

internal class CurveDividerService(SystemPluginConfig systemPluginConfig)
{
    private readonly double _tolerance = systemPluginConfig.DefaultTolerance;
    private readonly double _maxLenCurve = UnitUtils.ConvertToInternalUnits(
        systemPluginConfig.DefaultLengthSegmentMm, UnitTypeId.Millimeters);

    public List<Curve> DivideToShortCurves(List<Curve> curves)
    {
        if (curves == null || curves.Count == 0)
            return new List<Curve>();

        var divided = new List<Curve>(curves.Count * 2);
        foreach (var curve in curves)
            divided.AddRange(DivideToShortCurve(curve, _maxLenCurve));

        return divided;
    }

    public List<Curve> SplitCurvesAtIntersections(List<Curve> curves)
    {
        if (curves == null || curves.Count == 0)
            return new List<Curve>();

        int n = curves.Count;

        // Быстрее, чем Dictionary<Curve, List<double>>
        var splitParameters = new List<double>[n];
        for (int i = 0; i < n; i++)
            splitParameters[i] = new List<double>(4);

        // Кэш параметров начала/конца
        var starts = new double[n];
        var ends = new double[n];
        for (int i = 0; i < n; i++)
        {
            starts[i] = curves[i].GetEndParameter(0);
            ends[i] = curves[i].GetEndParameter(1);
        }

        // Размер ячейки индекса:
        // - не слишком маленький, иначе много bucket'ов
        // - не слишком большой, иначе много кандидатов
        double cell = Math.Max(_maxLenCurve, UnitUtils.ConvertToInternalUnits(200, UnitTypeId.Millimeters));
        if (cell <= 1e-9) cell = UnitUtils.ConvertToInternalUnits(50, UnitTypeId.Millimeters);

        var index = new CurveSpatialIndex(curves, cell);

        for (int i = 0; i < n; i++)
        {
            Curve c1 = curves[i];
            BBox b1 = index.Boxes[i];

            // Только пространственно близкие кандидаты
            foreach (int j in index.Query(b1))
            {
                if (j <= i) continue;

                Curve c2 = curves[j];

                // Быстрый bbox-отсев
                if (!BBoxesOverlap(index.Boxes[i], index.Boxes[j], _tolerance))
                    continue;

                // Пропускаем коллинеарные линии
                if (AreCollinearLines(c1, c2, _tolerance))
                    continue;

                IntersectionResultArray ira;
                try
                {
                    c1.Intersect(c2, out ira);
                }
                catch
                {
                    // В Revit API Intersect иногда кидает на плохой геометрии
                    continue;
                }

                if (ira == null || ira.Size == 0)
                    continue;

                for (int k = 0; k < ira.Size; k++)
                {
                    var ir = ira.get_Item(k);
                    XYZ point = ir?.XYZPoint;
                    if (point == null) continue;

                    var proj1 = c1.Project(point);
                    var proj2 = c2.Project(point);
                    if (proj1 == null || proj2 == null) continue;

                    double p1 = proj1.Parameter;
                    double p2 = proj2.Parameter;

                    bool inside1 = Math.Abs(p1 - starts[i]) > _tolerance &&
                                   Math.Abs(p1 - ends[i]) > _tolerance;

                    bool inside2 = Math.Abs(p2 - starts[j]) > _tolerance &&
                                   Math.Abs(p2 - ends[j]) > _tolerance;

                    if (inside1) splitParameters[i].Add(p1);
                    if (inside2) splitParameters[j].Add(p2);
                }
            }
        }

        var result = new List<Curve>(n * 2);
        for (int i = 0; i < n; i++)
            result.AddRange(SplitCurveByParameters(curves[i], splitParameters[i]));

        return result;
    }

    private static bool BBoxesOverlap(BBox a, BBox b, double tol)
    {
        return !(a.MaxX < b.MinX - tol ||
                 a.MinX > b.MaxX + tol ||
                 a.MaxY < b.MinY - tol ||
                 a.MinY > b.MaxY + tol);
    }

    private static bool AreCollinearLines(Curve l1, Curve l2, double tol)
    {
        if (l1 is not Line line1 || l2 is not Line line2)
            return false;

        XYZ d1 = line1.Direction.Normalize();
        XYZ d2 = line2.Direction.Normalize();

        // Параллельность
        if (d1.CrossProduct(d2).GetLength() > tol)
            return false;

        // Принадлежность одной прямой
        XYZ v = line2.GetEndPoint(0) - line1.GetEndPoint(0);
        return d1.CrossProduct(v).GetLength() <= tol;
    }

    private List<Curve> SplitCurveByParameters(Curve curve, List<double> parameters)
    {
        if (curve == null)
            return new List<Curve>();

        if (parameters == null || parameters.Count == 0)
            return new List<Curve> { curve };

        double start = curve.GetEndParameter(0);
        double end = curve.GetEndParameter(1);

        if (end < start)
            (start, end) = (end, start);

        parameters.Sort();

        // Удаление дублей и отсев концов
        var unique = new List<double>(parameters.Count);
        foreach (double p in parameters)
        {
            if (p <= start + _tolerance) continue;
            if (p >= end - _tolerance) continue;

            if (unique.Count == 0 || Math.Abs(unique[unique.Count - 1] - p) > _tolerance)
                unique.Add(p);
        }

        if (unique.Count == 0)
            return new List<Curve> { curve };

        var bounds = new List<double>(unique.Count + 2) { start };
        bounds.AddRange(unique);
        bounds.Add(end);

        var result = new List<Curve>(bounds.Count - 1);

        for (int i = 0; i < bounds.Count - 1; i++)
        {
            double p0 = bounds[i];
            double p1 = bounds[i + 1];
            if (Math.Abs(p1 - p0) < _tolerance)
                continue;

            var piece = curve.Clone();
            try
            {
                piece.MakeBound(p0, p1);
                if (piece.Length > _tolerance)
                    result.Add(piece);
            }
            catch
            {
                // пропускаем вырожденные куски
            }
        }

        return result;
    }

    private List<Curve> DivideToShortCurve(Curve curve, double maxLenCurve, int minSegments = 1)
    {
        if (curve == null)
            return new List<Curve>();

        int segments = minSegments <= 0 ? 1 : minSegments;

        return curve switch
        {
            Line line => DivideLine(line, maxLenCurve, segments),
            Arc arc => DivideArc(arc, maxLenCurve, segments),
            _ => new List<Curve> { curve }
        };
    }

    private List<Curve> DivideLine(Line line, double maxSegmentLength, int minSegments)
    {
        double len = line.Length;
        if (len <= _tolerance)
            return new List<Curve>();

        if (maxSegmentLength <= _tolerance)
            maxSegmentLength = len;

        int numberOfSegments = (int)Math.Ceiling(len / maxSegmentLength);
        int count = Math.Max(numberOfSegments, minSegments);

        XYZ start = line.GetEndPoint(0);
        XYZ end = line.GetEndPoint(1);

        var result = new List<Curve>(count);
        XYZ dir = end - start;

        for (int i = 0; i < count; i++)
        {
            double a = i / (double)count;
            double b = (i + 1) / (double)count;

            XYZ s = start + dir * a;
            XYZ e = start + dir * b;

            if (s.DistanceTo(e) <= _tolerance)
                continue;

            result.Add(Line.CreateBound(s, e));
        }

        return result;
    }

    private List<Curve> DivideArc(Arc arc, double maxSegmentLength, int minSegments)
    {
        double len = arc.Length;
        if (len <= _tolerance)
            return new List<Curve>();

        if (maxSegmentLength <= _tolerance)
            maxSegmentLength = len;

        int numberOfSegments = (int)Math.Ceiling(len / maxSegmentLength);
        int count = Math.Max(numberOfSegments, minSegments);

        double t0 = arc.GetEndParameter(0);
        double t1 = arc.GetEndParameter(1);

        var result = new List<Curve>(count);

        for (int i = 0; i < count; i++)
        {
            double a0 = i / (double)count;
            double a1 = (i + 1) / (double)count;

            double ta = t0 + (t1 - t0) * a0;
            double tb = t0 + (t1 - t0) * a1;
            double tm = (ta + tb) * 0.5;

            XYZ pStart = arc.Evaluate(ta, false);
            XYZ pMid = arc.Evaluate(tm, false);
            XYZ pEnd = arc.Evaluate(tb, false);

            if (pStart.DistanceTo(pEnd) <= _tolerance)
                continue;

            try
            {
                result.Add(Arc.Create(pStart, pEnd, pMid));
            }
            catch
            {
                // Иногда на почти вырожденных кусках может падать
            }
        }

        return result;
    }
}
