using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models;

internal class DirectShapeObjectFactory(SystemPluginConfig systemPluginConfig) : IDirectShapeObjectFactory {
    public List<DirectShapeObject> CreateDirectShapeObjects(List<GeomObject> geomObjects, RevitRepository revitRepository) {
        var directShapeElements = new List<DirectShapeObject>();
        foreach(var geomObject in geomObjects) {
            var directShapeElement = CreateDirectShapeObject(geomObject, revitRepository);
            if(directShapeElement != null) {
                directShapeElements.Add(directShapeElement);
            }
        }
        return directShapeElements;
    }

    // Метод построения DirectShapeObject
    private DirectShapeObject CreateDirectShapeObject(GeomObject geomObject, RevitRepository revitRepository) {
        var geometryObjects = geomObject.GeometryObjects;
        var directShape = DirectShape.CreateElement(revitRepository.Document, systemPluginConfig.ElementIdDirectShape);

        if(directShape.IsValidShape(geometryObjects)) {
            directShape.SetShape(geometryObjects);
            return new DirectShapeObject {
                DirectShape = directShape,
                FloorName = geomObject.LevelName,
                Volume = geomObject.Volume
            };
        } else {
            return null;
        }
    }
}
