using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitSplitMepCurve.Models.Exceptions;
using RevitSplitMepCurve.Models.Settings;

namespace RevitSplitMepCurve.Models.Splittable;

internal abstract class SplittableElement {
    protected SplittableElement(MEPCurve element, ICollection<DisplacementElement> displacementElements) {
        Element = element ?? throw new ArgumentNullException(nameof(element));
        DisplacementElements = displacementElements ?? [];
    }

    public MEPCurve Element { get; }

    /// <summary>Все DisplacementElement-ы, в которые входит Element.</summary>
    public ICollection<DisplacementElement> DisplacementElements { get; }

    /// <summary>true, если кривая пересекает хотя бы один уровень.</summary>
    public bool CanBeSplitted(ICollection<Level> levels) {
        if(levels.Count == 0) {
            return false;
        }

        double tolerance = Element.Document.Application.ShortCurveTolerance;

        (var p0, var p1) = GetEndPoints();
        double elMax = Math.Max(p0.Z, p1.Z);
        double elMin = Math.Min(p0.Z, p1.Z);

        double levelMax = levels.Max(l => l.Elevation) + tolerance;
        double levelMin = levels.Min(l => l.Elevation) - tolerance;
        return elMin < levelMin && levelMax < elMax;
    }

    /// <summary>Делит элемент и возвращает результат.</summary>
    public abstract SplitResult Split(ISplitSettings settings);

    protected (XYZ p0, XYZ p1) GetEndPoints() {
        var curve = ((LocationCurve) Element.Location).Curve;
        var p0 = curve.GetEndPoint(0);
        var p1 = curve.GetEndPoint(1);
        return (p0, p1);
    }

    protected Connector GetClosestConnector(MEPCurve mepEl, XYZ point) {
        return mepEl.ConnectorManager.Connectors
            .OfType<Connector>()
            .OrderBy(c => c.Origin.DistanceTo(point))
            .First();
    }

    /// <summary>
    /// Точки, по которым надо разбивать осевую линию ВИС элемента.
    /// Когда элемент ВИС делится - новый кусок будет создаваться со стороны GetEndPoint(0) точки линии.
    /// Поэтому при последовательном многократном делении одного и того же элемента надо делить в направлении от 0 к 1.</summary>
    protected IList<XYZ> GetIntersections(ICollection<Level> levels) {
        (var p0, var p1) = GetEndPoints();
        Level[] sortedLevels;
        XYZ curveMin, curveMax;
        if(p0.Z <= p1.Z) {
            sortedLevels = levels.OrderBy(l => l.Elevation).ToArray();
            (curveMin, curveMax) = (p0, p1);
        } else {
            sortedLevels = levels.OrderByDescending(l => l.Elevation).ToArray();
            (curveMin, curveMax) = (p1, p0);
        }

        var results = new List<XYZ>();
        for(int i = 0; i < sortedLevels.Length; i++) {
            var level = sortedLevels[i];
            if(!CanBeSplitted(level)) {
                continue;
            }

            double curveLengthZ = curveMax.Z - curveMin.Z;
            double proportion = (level.Elevation - curveMin.Z) / curveLengthZ;
            var point = curveMin + (curveMax - curveMin) * proportion;
            results.Add(point);
        }

        return results;
    }

    protected FamilyInstance InsertConnector(
        FamilySymbol connectorSymbol,
        MEPCurveType mepType,
        Connector mepEl1,
        Connector mepEl2) {
        const RoutingPreferenceRuleGroupType group = RoutingPreferenceRuleGroupType.Unions;
        var tempRule = new RoutingPreferenceRule(connectorSymbol.Id, "TEMP_UNION_RULE");

        mepType.RoutingPreferenceManager.AddRule(group, tempRule, 0);
        FamilyInstance createdConnector;
        try {
            createdConnector = mepType.Document.Create.NewUnionFitting(mepEl1, mepEl2);
        } catch(Autodesk.Revit.Exceptions.ArgumentException) {
            throw new CannotCreateConnectorException();
        } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
            throw new CannotCreateConnectorException();
        }

        mepType.RoutingPreferenceManager.RemoveRule(group, 0);
        return createdConnector;
    }

    private bool CanBeSplitted(Level level) {
        double tolerance = Element.Document.Application.ShortCurveTolerance;
        (var p0, var p1) = GetEndPoints();
        double elMax = Math.Max(p0.Z, p1.Z);
        double elMin = Math.Min(p0.Z, p1.Z);

        double levelMax = level.Elevation + tolerance;
        double levelMin = level.Elevation - tolerance;
        return elMin < levelMin && levelMax < elMax;
    }
}
