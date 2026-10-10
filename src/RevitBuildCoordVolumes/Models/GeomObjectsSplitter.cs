using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models;

internal class GeomObjectsSplitter : IGeomObjectsSplitter{

    private readonly ISplitSolidService _splitSolidService;
    
    public GeomObjectsSplitter(ISplitSolidService splitSolidService) {
        _splitSolidService = splitSolidService;
    }
    
    public List<GeomObject> SplitGeomObjects(List<GeomObject> geomObjects, ColumnGroupObject columnGroupObject, SpatialObject spatialObject, ProgressService progressService) {
        var solids = geomObjects
            .SelectMany(x => x.GeometryObjects
                .OfType<Solid>()
                .Select(solid => new SolidObject {
                    Solid = solid,
                    LevelName = x.LevelName
                }))
            .ToList();

        var splitSolids = _splitSolidService.SplitSolidObjects(solids, progressService);
        
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
