using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitVolumeModifier.Models;

internal class DirectShapeObjectFactory {
    private readonly SystemPluginConfig _systemPluginConfig;

    public DirectShapeObjectFactory(SystemPluginConfig systemPluginConfig) {
        _systemPluginConfig = systemPluginConfig;
    }

    public List<DirectShapeObject> GetDirectShapeObjects(List<GeomObject> geomObjects, Document document) {
        var directShapeElements = new List<DirectShapeObject>();
        foreach(var geomObject in geomObjects) {
            var directShapeElement = GetDirectShapeObject(geomObject, document);
            if(directShapeElement != null) {
                directShapeElements.Add(directShapeElement);
            }
        }
        return directShapeElements;
    }

    public DirectShapeObject GetDirectShapeObject(GeomObject geomObject, Document document) {
        var geometryObjects = geomObject.GeometryObjects;
        var directShape = DirectShape.CreateElement(document, _systemPluginConfig.ElementIdDirectShape);

        if(directShape.IsValidShape(geometryObjects)) {
            directShape.SetShape(geometryObjects);
            return new DirectShapeObject {
                DirectShape = directShape,
                Volume = geomObject.Volume
            };
        } else {
            return null;
        }
    }
}
