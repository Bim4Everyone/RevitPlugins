using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace RevitWindowGapPlacement.Model {
    internal abstract class BaseWindow : ICanPlaceWindowGap {
        private readonly Element _element;
        protected readonly RevitRepository _revitRepository;

        public BaseWindow(Element element, RevitRepository revitRepository) {
            _element = element;
            _revitRepository = revitRepository;
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