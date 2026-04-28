using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitSplitMepCurve.Models.Settings;

namespace RevitSplitMepCurve.Models.Splittable;

internal abstract class SplittableElement {
    protected SplittableElement(MEPCurve element, ICollection<DisplacementElement> displacementElements) {
        Element = element ?? throw new ArgumentNullException(nameof(element));
        DisplacementElements = displacementElements ?? Array.Empty<DisplacementElement>();
    }

    public MEPCurve Element { get; }

    /// <summary>Все DisplacementElement-ы, в которые входит Element.</summary>
    public ICollection<DisplacementElement> DisplacementElements { get; }

    /// <summary>true, если кривая пересекает хотя бы один уникальный уровень.</summary>
    public bool CanBeSplitted(ICollection<Level> levels) {
        return GetIntersections(levels).Count > 0;
    }

    /// <summary>Делит элемент и возвращает результат.</summary>
    public abstract SplitResult Split(ISplitSettings settings);

    /// <summary>Список (Level, точка пересечения) — снизу вверх; дубли не включены.</summary>
    protected IList<(Level Level, XYZ Point)> GetIntersections(ICollection<Level> levels) {
        var curve = ((LocationCurve)Element.Location).Curve;
        var p0 = curve.GetEndPoint(0);
        var p1 = curve.GetEndPoint(1);

        double zMin = Math.Min(p0.Z, p1.Z);
        double zMax = Math.Max(p0.Z, p1.Z);

        if(Math.Abs(zMax - zMin) < 1e-6) {
            return Array.Empty<(Level, XYZ)>();
        }

        var results = new List<(Level, XYZ)>();
        var seenZ = new HashSet<double>();

        foreach(var level in levels.OrderBy(l => l.Elevation)) {
            double z = level.Elevation;
            if(z <= zMin || z >= zMax) {
                continue;
            }

            double roundedZ = Math.Round(z, 4);
            if(!seenZ.Add(roundedZ)) {
                continue;
            }

            double t = (z - p0.Z) / (p1.Z - p0.Z);
            var point = new XYZ(
                p0.X + t * (p1.X - p0.X),
                p0.Y + t * (p1.Y - p0.Y),
                z);

            results.Add((level, point));
        }

        return results;
    }

    /// <summary>Группы уровней на одной отметке (с точностью до 4 знаков).</summary>
    protected IReadOnlyList<IReadOnlyList<Level>> GetDuplicateGroups(ICollection<Level> levels) {
        return levels
            .GroupBy(l => Math.Round(l.Elevation, 4))
            .Where(g => g.Count() > 1)
            .Select(g => (IReadOnlyList<Level>)g.ToArray())
            .ToArray();
    }

    /// <summary>Устанавливает WorksetId нового элемента по образцу исходного.</summary>
    protected static void CopyWorksetId(Document document, Element target, WorksetId sourceWorksetId) {
        if(!document.IsWorkshared) {
            return;
        }
        try {
            target.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM)
                  ?.Set(sourceWorksetId.IntegerValue);
        } catch {
            // Non-fatal: workset param may be read-only
        }
    }

    /// <summary>Находит ближайший открытый коннектор MEPCurve к заданной точке.</summary>
    protected static Connector GetOpenConnectorNear(MEPCurve curve, XYZ point) {
        Connector nearest = null;
        double minDist = double.MaxValue;
        foreach(Connector c in curve.ConnectorManager.Connectors) {
            if(c.IsConnected) {
                continue;
            }
            double dist = c.Origin.DistanceTo(point);
            if(dist < minDist) {
                minDist = dist;
                nearest = c;
            }
        }
        return nearest;
    }

    /// <summary>Подключает коннекторы семейства к открытым коннекторам кривых.</summary>
    protected static void ConnectFittingToCurves(FamilyInstance fitting, MEPCurve curve1, MEPCurve curve2, XYZ point) {
        Connector c1 = GetOpenConnectorNear(curve1, point);
        Connector c2 = GetOpenConnectorNear(curve2, point);
        if(c1 is null || c2 is null) {
            return;
        }

        Connector f1 = null, f2 = null;
        double minD1 = double.MaxValue, minD2 = double.MaxValue;
        foreach(Connector fc in fitting.MEPModel.ConnectorManager.Connectors) {
            double d1 = fc.Origin.DistanceTo(c1.Origin);
            double d2 = fc.Origin.DistanceTo(c2.Origin);
            if(d1 < minD1) { minD1 = d1; f1 = fc; }
            if(d2 < minD2) { minD2 = d2; f2 = fc; }
        }

        if(f1 is not null && !f1.IsConnected) { c1.ConnectTo(f1); }
        if(f2 is not null && f2 != f1 && !f2.IsConnected) { c2.ConnectTo(f2); }
    }
}
