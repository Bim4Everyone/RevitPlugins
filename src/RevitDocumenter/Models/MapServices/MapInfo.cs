using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.MapServices;
internal class MapInfo {
    public MapInfo(SquareInfo[,] map, string path, double mappingStepInFeet, int stepCountX, int stepCountY,
        XYZ startPointInRevit) {
        Map = map;
        ImagePath = path;
        MappingStepInFeet = mappingStepInFeet;
        StepCountX = stepCountX;
        StepCountY = stepCountY;
        StartPointInRevit = startPointInRevit;
    }

    public SquareInfo[,] Map { get; }
    public string ImagePath { get; }
    public double MappingStepInFeet { get; }
    public int StepCountX { get; }
    public int StepCountY { get; }
    public XYZ StartPointInRevit { get; }
}
