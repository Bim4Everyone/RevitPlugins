using Autodesk.Revit.DB;

namespace RevitSetLevelSection.Models.LevelDefinitions {
    internal class BBMiddle : IBBPosition {
        public double GetPosition(Outline outline) {
            return (outline.MaximumPoint.Z - outline.MinimumPoint.Z) / 2 + outline.MinimumPoint.Z;
        }
    }
}