using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Services;

namespace RevitBuildCoordVolumes.Models;

internal class GeomObjectsSplitter(ISplitSolidService splitSolidService) : IGeomObjectsSplitter {
    public List<GeomObject> SplitGeomObjects(List<GeomObject> geomObjects, ProgressService progressService) {
        var solids = geomObjects
            .SelectMany(x => x.GeometryObjects
                .OfType<Solid>()
                .Select(solid => new SolidObject {
                    Solid = solid,
                    LevelName = x.LevelName
                }))
            .ToList();

        var splitSolids = splitSolidService.SplitSolidObjects(solids, progressService);
        
        if(splitSolids.Count == 0) {
            return [];
        }

        return splitSolids
            .Select(solidObject => new GeomObject {
                GeometryObjects = [solidObject.Solid],
                LevelName = solidObject.LevelName,
                Volume = solidObject.Solid.Volume
            })
            .ToList();
    }
}
