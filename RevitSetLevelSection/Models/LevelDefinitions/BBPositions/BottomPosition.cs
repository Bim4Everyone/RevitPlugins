using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitSetLevelSection.Models.LevelDefinitions.BBPositions {
    internal class BBPositionBottom : IBBPosition {
        public double GetPosition(Element element) {
            return element.GetBoundingBox()?.Min.Z ?? 0;
        }
    }
}