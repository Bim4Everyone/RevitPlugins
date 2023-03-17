using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitSetLevelSection.Models.ElementPositions {
    internal class ElementTopPosition : IElementPosition {
        public double GetPosition(Element element) {
            return element.GetBoundingBox()?.Max.Z ?? 0;
        }
    }
}