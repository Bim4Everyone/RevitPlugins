using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

public static class CurveSplitUtils
{
    /// <summary>
    /// Разбивает Curve на короткие Curve.
    /// - Line режется на несколько Line.
    /// - Arc режется на несколько Arc (не аппроксимация линиями).
    /// Для других типов можно добавить ветку или упасть с исключением.
    /// </summary>
    /// <param name="curve">Исходная кривая (Line или Arc).</param>
    /// <param name="maxSegmentLength">Макс. длина сегмента (internal units).</param>
    /// <param name="minSegments">Минимум сегментов.</param>
    public static List<Curve> SplitToShortCurves(Curve curve, double maxSegmentLength, int minSegments = 1)
    {
        if (curve == null) throw new ArgumentNullException(nameof(curve));
        if (maxSegmentLength <= 0) throw new ArgumentException("maxSegmentLength must be > 0", nameof(maxSegmentLength));
        if (minSegments <= 0) minSegments = 1;

        if (curve is Line line)
            return SplitLine(line, maxSegmentLength, minSegments);

        if (curve is Arc arc)
            return SplitArc(arc, maxSegmentLength, minSegments);

        throw new NotSupportedException($"Curve type '{curve.GetType().Name}' is not supported. Only Line and Arc.");
    }

    private static List<Curve> SplitLine(Line line, double maxSegmentLength, int minSegments)
    {
        double len = line.Length;
        if (len <= 1e-9) return new List<Curve>();

        int n = (int)Math.Ceiling(len / maxSegmentLength);
        n = Math.Max(n, minSegments);

        XYZ p0 = line.GetEndPoint(0);
        XYZ p1 = line.GetEndPoint(1);

        var res = new List<Curve>(n);
        for (int i = 0; i < n; i++)
        {
            double a = i / (double)n;
            double b = (i + 1) / (double)n;

            XYZ s = p0 + (p1 - p0) * a;
            XYZ e = p0 + (p1 - p0) * b;

            if (s.DistanceTo(e) > 1e-9)
                res.Add(Line.CreateBound(s, e));
        }
        return res;
    }

    private static List<Curve> SplitArc(Arc arc, double maxSegmentLength, int minSegments)
    {
        double len = arc.Length;
        if (len <= 1e-9) return new List<Curve>();

        int n = (int)Math.Ceiling(len / maxSegmentLength);
        n = Math.Max(n, minSegments);

        // Параметрический домен дуги
        double t0 = arc.GetEndParameter(0);
        double t1 = arc.GetEndParameter(1);

        var res = new List<Curve>(n);

        // Чтобы корректно создавать поддуги, берём три точки на каждой части:
        // start, mid, end. Тогда Arc.Create(start, end, mid) создаст нужную дугу.
        for (int i = 0; i < n; i++)
        {
            double a0 = i / (double)n;
            double a1 = (i + 1) / (double)n;

            double ta = t0 + (t1 - t0) * a0;
            double tb = t0 + (t1 - t0) * a1;
            double tm = (ta + tb) * 0.5;

            XYZ pStart = arc.Evaluate(ta, false);
            XYZ pMid   = arc.Evaluate(tm, false);
            XYZ pEnd   = arc.Evaluate(tb, false);

            if (pStart.DistanceTo(pEnd) <= 1e-9)
                continue;

            // Создаём поддугу через 3 точки
            Arc subArc = Arc.Create(pStart, pEnd, pMid);
            res.Add(subArc);
        }

        return res;
    }
}
