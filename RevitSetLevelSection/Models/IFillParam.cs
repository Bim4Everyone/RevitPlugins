using Autodesk.Revit.DB;

namespace RevitSetLevelSection.Models {
    internal interface IFillParam {
        void UpdateValue(Element element);
    }
}