using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Enums;

namespace RevitBuildCoordVolumes.Models.Settings;

internal class BuildCoordVolumesSettings {
    public AlgorithmType AlgorithmType { get; set; }
    public string TypeZone { get; set; }
    public List<ParamMap> ParamMaps { get; set; }
    public List<Document> Documents { get; set; }
    public List<string> TypeSlabs { get; set; }
    public Level UpLevel { get; set; }
    public Level BottomLevel { get; set; }
    public double SquareSideMm { get; set; }
}
