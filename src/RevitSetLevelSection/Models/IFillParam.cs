using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

namespace RevitSetLevelSection.Models {
    internal interface IFillParam {
        RevitParam RevitParam { get; }
        void UpdateValue(Element element);
    }
}