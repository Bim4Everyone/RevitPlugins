using Autodesk.Revit.DB;

namespace RevitSetLevelSection.Models.LevelDefinitions {
    internal class BBPositionTop : IBBPosition {
        public double GetPosition(Outline outline) {
            return outline.MaximumPoint.Z;
        }
    }
}