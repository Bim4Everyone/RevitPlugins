using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitVolumeModifier.Models;
internal class GeomObjectFactory {
    public GeomObject GetGeomObject(List<Solid> solids) {
        var geomObjects = new List<GeometryObject>();
        var volumes = new List<double>();
        foreach(var solid in solids) {
            if(solid != null) {
                geomObjects.Add(solid);
                volumes.Add(GetSafeSolidVolume(solid));
            }
        }
        return geomObjects.Count == 0 || volumes.Count == 0
            ? null
            : new GeomObject {
                GeometryObjects = geomObjects,
                Volume = volumes.Sum()
            };
    }

    // Метод безопасного получения объёма солида
    private double GetSafeSolidVolume(Solid solid) {
        if(solid == null) {
            return 0;
        }
        try {
            return solid.Volume;
        } catch {
            return 0;
        }
    }
}
