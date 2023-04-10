using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

namespace RevitSetLevelSection.Models {
    internal class FillAdskParam : IFillParam {
        public RevitParam RevitParam { get; }

        public void UpdateValue(Element element) {
            throw new System.NotImplementedException();
        }
    }
}