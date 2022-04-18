using Autodesk.Revit.DB;

namespace RevitSetLevelSection.Models.LevelDefinitions {
    internal interface IBBPosition {
        double GetPosition(Outline outline);
    }
}