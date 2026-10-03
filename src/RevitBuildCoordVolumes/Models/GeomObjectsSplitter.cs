using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models;

internal class GeomObjectsSplitter : IGeomObjectsSplitter{
    public List<GeomObject> SplitGeomObjects(List<GeomObject> geomObjects, List<ColumnObject> columns, SpatialObject spatialObject) {
        var firstElement = columns[0];
        var solids = geomObjects
            .SelectMany(x => x.GeometryObjects)
            .OfType<Solid>()
            .ToList();

        var splitSolids = SolidUtility.Split(solids);
        
        if(splitSolids.Count == 0) {
            return [];
        }

        return splitSolids
            .Select(solid => new GeomObject {
                GeometryObjects = [solid],
                LevelName = firstElement.LevelName,
                Volume = solid.Volume
            })
            .ToList();
    }
}
