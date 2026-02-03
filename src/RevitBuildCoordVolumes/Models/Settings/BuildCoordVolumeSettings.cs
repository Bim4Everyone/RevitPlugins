using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Settings;

internal class BuildCoordVolumeSettings {
    public AlgorithmType AlgorithmType { get; set; }
    public BuilderMode BuilderMode { get; set; }
    public string TypeZone { get; set; }
    public List<ParamMap> ParamMaps { get; set; }
    public List<Document> Documents { get; set; }
    public List<string> TypeSlabs { get; set; }
    public List<Level> Levels { get; set; }
    public double SquareSideMm { get; set; }
    public double SquareAngleDeg { get; set; }
    public List<SpatialObject> SpatialObjects { get; set; }
}
