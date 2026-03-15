using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitSetCoordParams.Models.Settings;

namespace RevitSetCoordParams.Models;
internal class CurveIntersector {
    private readonly SetCoordParamsSettings _settings;
    private readonly ICollection<RevitElement> _sourceModels;
    private readonly double _intersectCurveLength;
    private readonly double _startDiamSphere;
    private readonly double _maxDiamSphere;
    private readonly double _stepDiamSphere;
    private readonly SolidCurveIntersectionOptions _intersectOptions;

    public CurveIntersector(SetCoordParamsSettings settings) {
        _settings = settings;
        _sourceModels = _settings.TypeModels
            .SelectMany(_settings.FileProvider.GetRevitElements)
            .Where(x => x.BoundingBoxXYZ != null)
            .ToArray();
        _intersectCurveLength = UnitUtils.ConvertToInternalUnits(RevitConstants.IntersectCurveLengthMm, UnitTypeId.Millimeters);
        _startDiamSphere = UnitUtils.ConvertToInternalUnits(RevitConstants.StartDiameterSearchSphereMm, UnitTypeId.Millimeters);
        _maxDiamSphere = UnitUtils.ConvertToInternalUnits(_settings.MaxDiameterSearchSphereMm, UnitTypeId.Millimeters);
        _stepDiamSphere = UnitUtils.ConvertToInternalUnits(_settings.StepDiameterSearchSphereMm, UnitTypeId.Millimeters);
        _intersectOptions = new();
    }

    /// <summary>
    /// Основной метод пересечения.
    /// </summary>    
    /// <remarks>
    /// В данном методе происходит пересечение объемных моделей и элемента основного файла.
    /// </remarks>
    /// <returns>Возвращает RevitElement, который пересекся с элементом модели либо null.</returns> 
    public RevitElement Intersect(RevitElement targetElement) {
        if(targetElement.BoundingBoxXYZ is null) {
            return null;
        }

        var position = _settings.PositionProvider.GetPositionElement(targetElement);

        var containCandidates = GetContainCandidates(position).ToList();

        if(containCandidates.Count > 0) {
            var offset = new XYZ(0, 0, _intersectCurveLength);
            var curve = GetIntersectCurve(position, offset);
            var foundModel = IntersectWithCurve(curve, containCandidates);

            if(foundModel is not null) {
                return foundModel;
            }
        }

        if(!_settings.Search) {
            return null;
        }

        double currentDiamSphere = _startDiamSphere;
        List<RevitElement> candidates = null;

        while(currentDiamSphere < _maxDiamSphere) {
            var sphereOutline = GetSphereOutline(position, currentDiamSphere);
            candidates = GetIntersectCandidates(sphereOutline).ToList();

            if(candidates.Count == 0) {
                currentDiamSphere += _stepDiamSphere;
                continue;
            }

            var curves = GetSphereLine(position, currentDiamSphere);
            var foundModel = IntersectWithCurves(curves, candidates);

            if(foundModel is not null) {
                return foundModel;
            }

            currentDiamSphere += _stepDiamSphere;
        }
        return null;
    }

    // Метод отсеивания кандидатов-объёмных моделей путем нахождения XYZ в Outline
    private IEnumerable<RevitElement> GetContainCandidates(XYZ position) {
        foreach(var sourceModel in _sourceModels) {
            if(sourceModel.Outline.Contains(position, 0)) {
                yield return sourceModel;
            }
        }
    }

    // Метод отсеивания кандидатов-объёмных моделей путем пересечения BoundingBox и Outline    
    private IEnumerable<RevitElement> GetIntersectCandidates(Outline sphereOutline) {
        foreach(var sourceModel in _sourceModels) {
            if(sourceModel.Outline.Intersects(sphereOutline, 0)) {
                yield return sourceModel;
            }
        }
    }

    // Метод расширения сферы-Outline 
    private Outline GetSphereOutline(XYZ position, double diamSphere) {
        double radius = diamSphere / 2.0;
        var sphereMin = new XYZ(position.X - radius, position.Y - radius, position.Z - radius);
        var sphereMax = new XYZ(position.X + radius, position.Y + radius, position.Z + radius);
        return new Outline(sphereMin, sphereMax);
    }

    // Метод пересечения линии с объемными элементами
    private RevitElement IntersectWithCurve(Curve curve, IEnumerable<RevitElement> candidates) {
        foreach(var sourceModel in candidates) {
            var solid = sourceModel.Solid;
            if(solid is null) {
                continue;
            }
            var result = solid.IntersectWithCurve(curve, _intersectOptions);

            if(result.ResultType is not SolidCurveIntersectionMode.CurveSegmentsInside) {
                continue;
            }
            int segmentsCount = result.SegmentCount;
            for(int i = 0; i < segmentsCount; i++) {
                var segment = result.GetCurveSegment(i);
                if(segment is not null && segment.Length > 0) {
                    return sourceModel;
                }
            }
        }
        return null;
    }

    // Метод пересечения сферы из линий с объемными элементами
    private RevitElement IntersectWithCurves(List<Curve> curves, IEnumerable<RevitElement> candidates) {
        foreach(var curve in curves) {
            var result = IntersectWithCurve(curve, candidates);
            if(result is not null) {
                return result;
            }
        }
        return null;
    }

    // Метод получения кривой для пересечения с объемным элементом        
    private Curve GetIntersectCurve(XYZ origin, XYZ offset) {
        return Line.CreateBound(origin, origin + offset);
    }

    // Метод получения сферы из линий   
    private List<Curve> GetSphereLine(XYZ origin, double diameter) {
        double r = diameter / 2.0;
        var top = new XYZ(origin.X, origin.Y, origin.Z + r);
        var bottom = new XYZ(origin.X, origin.Y, origin.Z - r);
        var right = new XYZ(origin.X + r, origin.Y, origin.Z);
        var left = new XYZ(origin.X - r, origin.Y, origin.Z);
        var front = new XYZ(origin.X, origin.Y + r, origin.Z);
        var back = new XYZ(origin.X, origin.Y - r, origin.Z);

        var curves = new List<Curve>(6);
        {
            curves.Add(Arc.Create(bottom, top, right));
            curves.Add(Arc.Create(top, bottom, left));
            curves.Add(Arc.Create(bottom, top, front));
            curves.Add(Arc.Create(top, bottom, back));
            curves.Add(Arc.Create(left, right, front));
            curves.Add(Arc.Create(right, left, back));
        }
        return curves;
    }
}
