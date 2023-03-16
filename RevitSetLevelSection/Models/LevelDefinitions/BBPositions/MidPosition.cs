using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

namespace RevitSetLevelSection.Models.LevelDefinitions.BBPositions {
    internal class BBPositionMiddle : IBBPosition {
        public double GetPosition(Element element) {
            return element.GetBoundingBox()?.GetMidPoint().Z ?? 0;
        }
    }
}