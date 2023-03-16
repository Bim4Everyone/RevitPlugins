using Autodesk.Revit.DB;

namespace RevitSetLevelSection.Models.LevelDefinitions.BBPositions {
    internal interface IBBPosition {
        double GetPosition(Element element);
    }
}