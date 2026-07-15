using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitAreaBoundaries.Models;

namespace RevitAreaBoundaries.Services;

internal class BoundingBoxService (RevitRepository revitRepository){
    private readonly Options _geomOptions = new() {
        ComputeReferences = false,
        DetailLevel = ViewDetailLevel.Fine
    };
    
    // Метод получения BoundingBoxXYZ
    public BoundingBoxXYZ GetBoundingBoxXyz(Element element) {
        if(element is not FamilyInstance familyInstance) {
            return GetElementBoundingBox(element);
        }
        var bbox = GetFamilyInstanceBoundingBox(familyInstance);
        bbox ??= GetElementBoundingBox(element);
        return bbox;
    }
    
    // Метод получения BoundingBoxXYZ загружаемых семейств
    private BoundingBoxXYZ GetFamilyInstanceBoundingBox(FamilyInstance familyInstance) {
        var geomElement = familyInstance.get_Geometry(_geomOptions);
        if(geomElement is null) {
            return null;
        }
        var list = new List<XYZ>();
        foreach(var geomObj in geomElement) {
            if(geomObj is GeometryInstance instance) {
                var instGeom = instance.GetInstanceGeometry();
                foreach(var obj in instGeom) {
                    if(obj is Solid solid && solid.Faces.Size > 0) {
                        foreach(Edge e in solid.Edges) {
                            var st = e.AsCurve().GetEndPoint(0);
                            var fi = e.AsCurve().GetEndPoint(1);
                            list.Add(st);
                            list.Add(fi);
                        }
                    }
                }
            }
        }
        if(list.Count == 0) {
            return null;
        }
        var boundingBox = CreatePointsBBox(list);
        var transformedBoundingBox = GetTransformedBoundingBox(boundingBox, revitRepository.BasePointTransform.Multiply(boundingBox.Transform));

        return transformedBoundingBox;
    }
    
    // Метод получения BoundingBoxXYZ из списка точек
    private BoundingBoxXYZ CreatePointsBBox(List<XYZ> points) {
        (var minPoint, var maxPoint) = GetMinMaxPoints(points);
        return new BoundingBoxXYZ {
            Min = minPoint,
            Max = maxPoint
        };
    }
    
    // Метод получения минимальной и максимальной точек из списка точек
    private (XYZ minPoint, XYZ maxPoint) GetMinMaxPoints(List<XYZ> points) {
        double minX = double.MaxValue;
        double minY = double.MaxValue;
        double minZ = double.MaxValue;
        double maxX = double.MinValue;
        double maxY = double.MinValue;
        double maxZ = double.MinValue;

        foreach(var point in points) {
            minX = Math.Min(minX, point.X);
            minY = Math.Min(minY, point.Y);
            minZ = Math.Min(minZ, point.Z);

            maxX = Math.Max(maxX, point.X);
            maxY = Math.Max(maxY, point.Y);
            maxZ = Math.Max(maxZ, point.Z);
        }
        var min = new XYZ(minX, minY, minZ);
        var max = new XYZ(maxX, maxY, maxZ);
        return (min, max);
    }
    
    // Метод получения BoundingBoxXYZ системных семейств
    private BoundingBoxXYZ GetElementBoundingBox(Element element) {
        var geomElement = element.get_Geometry(_geomOptions);
        if(geomElement is not null) {
            var geomBoundingBox = geomElement.GetBoundingBox();
            return GetTransformedBoundingBox(geomBoundingBox, revitRepository.BasePointTransform.Multiply(geomBoundingBox.Transform));
        }
        var boundingBox = element.GetBoundingBox();
        return boundingBox is not null
            ? GetTransformedBoundingBox(boundingBox, revitRepository.BasePointTransform.Multiply(boundingBox.Transform))
            : null;
    }
    
    // Метод получения трансформированного BoundingBoxXYZ
    private BoundingBoxXYZ GetTransformedBoundingBox(BoundingBoxXYZ bbox, Transform transform) {
        var min = bbox.Min;
        var max = bbox.Max;

        double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

        for(int x = 0; x <= 1; x++) {
            for(int y = 0; y <= 1; y++) {
                for(int z = 0; z <= 1; z++) {
                    var p = new XYZ(
                        x == 0 ? min.X : max.X,
                        y == 0 ? min.Y : max.Y,
                        z == 0 ? min.Z : max.Z);

                    var tp = transform.OfPoint(p);

                    minX = Math.Min(minX, tp.X);
                    minY = Math.Min(minY, tp.Y);
                    minZ = Math.Min(minZ, tp.Z);

                    maxX = Math.Max(maxX, tp.X);
                    maxY = Math.Max(maxY, tp.Y);
                    maxZ = Math.Max(maxZ, tp.Z);
                }
            }
        }

        return new BoundingBoxXYZ {
            Min = new XYZ(minX, minY, minZ),
            Max = new XYZ(maxX, maxY, maxZ)
        };
    }
    
}
