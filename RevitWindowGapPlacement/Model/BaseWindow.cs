using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace RevitWindowGapPlacement.Model {
    internal abstract class BaseWindow : ICanPlaceWindowGap {
        private readonly Element _element;

        public BaseWindow(Element element) {
            _element = element;
        }
        
        protected abstract XYZ GetLocation();
        protected abstract FamilyInstance UpdateParamsWindowGap(FamilyInstance windowGap);
        
        public FamilyInstance PlaceWindowGap(Document document, FamilySymbol windowGapType) {
            Element host = GetHost();
            XYZ location = GetLocation();
            
            FamilyInstance windowGap = document.Create.NewFamilyInstance(location, windowGapType, host, StructuralType.NonStructural);
            return UpdateParamsWindowGap(windowGap);
        }


        protected virtual Element GetHost() {
            return null;
        }
    }
}