using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Services;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface ISlabContourSolidService {
    Solid CreateFlatSlabContourSolid(
        ColumnObject columnObject,
        Solid spatialSolid,
        ProgressService progressService);
    
    IList<Solid> CreateSlopedSlabContourSolids(
        ColumnObject columnObject, 
        Solid spatialSolid, 
        ProgressService progressService);
}
