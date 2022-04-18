using Autodesk.Revit.DB;

namespace RevitSetLevelSection.Models.LevelDefinitions {
    internal class BBBottom : IBBPosition {
        public double GetPosition(Outline outline) {
            return outline.MinimumPoint.Z;
        }
    }
}