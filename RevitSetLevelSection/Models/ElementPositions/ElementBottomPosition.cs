using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitSetLevelSection.Models.ElementPositions {
    internal class ElementBottomPosition : IElementPosition {
        public double GetPosition(Element element) {
            return element.GetBoundingBox()?.Min.Z ?? 0;
        }
    }
}