using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models;

internal class DirectShapeObjectFactory : IDirectShapeObjectFactory {
    private readonly SystemPluginConfig _systemPluginConfig;

    public DirectShapeObjectFactory(SystemPluginConfig systemPluginConfig) {
        _systemPluginConfig = systemPluginConfig;
    }

    public List<DirectShapeObject> GetDirectShapeObjects(List<GeomObject> geomObjects, RevitRepository revitRepository) {
        var directShapeElements = new List<DirectShapeObject>();
        foreach(var geomObject in geomObjects) {
            var directShapeElement = GetDirectShapeObject(geomObject, revitRepository);
            if(directShapeElement != null) {
                directShapeElements.Add(directShapeElement);
            }
        }
        return directShapeElements;
    }

    // Метод построения DirectShapeObject
    private DirectShapeObject GetDirectShapeObject(GeomObject geomObject, RevitRepository revitRepository) {
        var geometryObjects = geomObject.GeometryObjects;
        var directShape = DirectShape.CreateElement(revitRepository.Document, _systemPluginConfig.ElementIdDirectShape);

        if(directShape.IsValidShape(geometryObjects)) {
            directShape.SetShape(geometryObjects);
            return new DirectShapeObject {
                DirectShape = directShape,
                FloorName = geomObject.FloorName,
                Volume = geomObject.Volume
            };
        } else {
            return null;
        }
    }
}
