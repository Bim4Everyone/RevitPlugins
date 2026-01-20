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

    public List<DirectShapeObject> GetDirectShapeElements(List<GeomObject> geomElements, RevitRepository revitRepository) {
        var directShapeElements = new List<DirectShapeObject>();
        foreach(var geomElement in geomElements) {
            var directShapeElement = GetDirectShapeElement(geomElement, revitRepository);
            if(directShapeElement != null) {
                directShapeElements.Add(directShapeElement);
            }
            ;
        }
        return directShapeElements;
    }

    private DirectShapeObject GetDirectShapeElement(GeomObject geomElement, RevitRepository revitRepository) {
        var geometryObjects = geomElement.GeometryObjects;
        var directShape = DirectShape.CreateElement(revitRepository.Document, _systemPluginConfig.ElementIdDirectShape);

        if(directShape.IsValidShape(geometryObjects)) {
            directShape.SetShape(geometryObjects);
            return new DirectShapeObject { DirectShape = directShape, FloorName = geomElement.FloorName };
        } else {
            return null;
        }
    }
}
