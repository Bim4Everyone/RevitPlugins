using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitSetLevelSection.Models.LevelDefinitions.BBPositions {
    internal class BBPositionTop : IBBPosition {
        public double GetPosition(Element element) {
            return element.GetBoundingBox()?.Max.Z ?? 0;
        }
    }
}