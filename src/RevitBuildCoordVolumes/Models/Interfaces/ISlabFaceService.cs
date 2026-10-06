using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Interfaces;

public interface ISlabFaceService {
    List<Face> GetRealTopFaces(Floor floor, Transform transformFromDoc);
    List<Face> GetFlatTopFaces(IEnumerable<(CurveLoop Loop, Solid Solid)> solids);
    double GetMaxPointZ(List<Face> topFaces);
    double GetMinPointZ(List<Face> topFaces);
}
