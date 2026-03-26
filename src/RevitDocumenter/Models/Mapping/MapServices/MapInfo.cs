using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Mapping.MapServices;
internal class MapInfo {
    public MapInfo(SquareInfo[,] map, string path, double mappingStepInFeet, int stepCountX, int stepCountY,
        XYZ startPointInRevit) {
        Map = map.ThrowIfNull();
        ImagePath = path.ThrowIfNullOrEmpty();
        MappingStepInFeet = mappingStepInFeet;
        StepCountX = stepCountX;
        StepCountY = stepCountY;
        StartPointInRevit = startPointInRevit.ThrowIfNull();
    }

    public SquareInfo[,] Map { get; }
    public string ImagePath { get; }
    public double MappingStepInFeet { get; }
    public int StepCountX { get; }
    public int StepCountY { get; }
    public XYZ StartPointInRevit { get; }
}
