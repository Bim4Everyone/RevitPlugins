using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

namespace RevitSetLevelSection.Models.ElementPositions {
    internal class ElementMiddlePosition : IElementPosition {
        public double GetPosition(Element element) {
            return element.GetBoundingBox()?.GetMidPoint().Z ?? 0;
        }
    }
}