using Autodesk.Revit.DB;

namespace RevitSetLevelSection.Models {
    internal interface IElementPosition {
        double GetPosition(Element element);
    }
}