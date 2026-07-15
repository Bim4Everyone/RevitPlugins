using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitAreaBoundaries.Models;
using RevitAreaBoundaries.Models.Enums;
using RevitAreaBoundaries.Settings;

namespace RevitAreaBoundaries.Services;

internal class ElementSectionService(
    RevitRepository revitRepository, 
    SystemPluginConfig systemPluginConfig, 
    BoundingBoxService boundingBoxService) {

    public List<Curve> GetSectionCurves(View view, AreaBoundarySettings areaBoundarySettings) {
        var level = view.GenLevel;
        double elevation = level.Elevation;
        double sectionHeight = UnitUtils.ConvertToInternalUnits(areaBoundarySettings.SectionHeightMm, UnitTypeId.Millimeters);
        double sectionHeightOffset = UnitUtils.ConvertToInternalUnits(systemPluginConfig.DefaultSectionHeightOffsetMm, UnitTypeId.Millimeters);
        double firstSection = elevation + sectionHeight;
        double secondSection = firstSection + sectionHeightOffset;
        
        var types = areaBoundarySettings.Types;
        var elementsOnView = revitRepository.GetElementsOnView(view, types);
        
        var typeKinds = types
            .Cast<RevitElementType>()
            .ToDictionary(x => x.Element.Id, x => x.ProjectionType );
        
        var resultCurves = new List<Curve>();
        foreach(var element in elementsOnView) {
            var typeId = element.GetTypeId();
            if(!typeKinds.TryGetValue(typeId, out var projectionType)) {
                continue;
            }
            switch(projectionType) {
                case ProjectionType.FullProjection:
                    resultCurves.AddRange(GetFullProjectionCurves(element));
                    break;
                case ProjectionType.PartialProjection:
                    resultCurves.AddRange(GetPartalProjectionCurves(element, sectionHeightOffset));
                    break;
                case ProjectionType.RegularProjection:
                default:
                    resultCurves.AddRange(GetRegularProjectionCurves(element, firstSection, secondSection));
                    break;
            }
        }
        return resultCurves;
        
    }
    
    private IEnumerable<Curve> GetFullProjectionCurves(Element element) {
        var solid = GetTransformedSolidFromElement(element);
        return solid == null 
            ? [] 
            : GetCurvesFromSolid(solid);
    }
    
    private IEnumerable<Curve> GetPartalProjectionCurves(Element element, double offset) {
        var solid = GetTransformedSolidFromElement(element);
        if(solid is null) {
            return [];
        }
        var bbox = boundingBoxService.GetBoundingBoxXyz(element);
        double minPoint = bbox.Min.Z;
        double offsetMinPoint = minPoint + offset;
        var minPlane = CreateCutPlaneByOrigin(offsetMinPoint, true);
        double maxPoint = bbox.Max.Z;
        double offsetMaxPoint = maxPoint - offset;
        var maxPlane = CreateCutPlaneByOrigin(offsetMaxPoint, true);
        return GetCurvesFromSolid(solid, minPlane, maxPlane);
    }
    
    private IEnumerable<Curve> GetRegularProjectionCurves(Element element, double firstSection, double secondSection) {
        var solid =  GetTransformedSolidFromElement(element);
        var firstPlane = CreateCutPlaneByOrigin(firstSection, true);
        var secondPlane = CreateCutPlaneByOrigin(secondSection, false);
        return solid == null 
            ? [] 
            : GetCurvesFromSolid(solid, firstPlane, secondPlane);
    }
    
    // Метод создания плоскости для обрезки
    private Plane CreateCutPlaneByOrigin(double positionZ, bool isPositive) {
        var origin = new XYZ(0, 0, positionZ);
        var direction = isPositive
            ? new XYZ(0, 0, 1)
            : new XYZ(0, 0, -1);
        return Plane.CreateByNormalAndOrigin(direction, origin
        );
    }

    // Метод получения кривых из Solid
    private IEnumerable<Curve> GetCurvesFromSolid(Solid solid) {
        var downFaces = new List<Face>();
        
        foreach(Face face in solid.Faces) {
            if(face is not PlanarFace planarFace) {
                continue;
            }
            if(Math.Abs(planarFace.FaceNormal.Z + 1.0) < systemPluginConfig.DefaultTolerance) {
                downFaces.Add(planarFace);
            }
        }
        
        var curves = new List<Curve>();
        foreach(var face in downFaces) {
            foreach(EdgeArray loop in face.EdgeLoops) {
                foreach(Edge edge in loop) {
                    curves.Add(edge.AsCurve());
                }
            }
        }
        return curves;
    }
    
    // Метод получения кривых из Solid путем обрезки исходного Solid
    private List<Curve> GetCurvesFromSolid(Solid solid, Plane positivePlane, Plane negativePlane) {
        var curves = new List<Curve>();
        try {
            if(solid == null) {
                return curves;
            }
        
            var resultSolid = BooleanOperationsUtils.CutWithHalfSpace(solid, positivePlane);
            if(resultSolid == null) {
                return curves;
            }
        
            var finalSolid = BooleanOperationsUtils.CutWithHalfSpace(resultSolid, negativePlane);
            
            return finalSolid == null 
                ? curves 
                : GetCurvesFromSolid(finalSolid).ToList();
            
        } catch {
            // ignored
        }
        return curves;
    }
    
    // Метод получения трансформируемого Solid
    private Solid GetTransformedSolidFromElement(Element element) {
        var unitedSolid = GetUnitedSolid(element);
        return unitedSolid == null 
            ? null 
            : SolidUtils.CreateTransformed(unitedSolid, revitRepository.BasePointTransform);
    }
    
    // Метод получения объединенного Solid
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

    // Метод безопасного получения объема Solid
    private double GetSafeSolidVolume(Solid solid) {
        if(solid == null) return 0;
        try {
            return solid.Volume;
        } catch {
            return 0;
        }
    }
}
