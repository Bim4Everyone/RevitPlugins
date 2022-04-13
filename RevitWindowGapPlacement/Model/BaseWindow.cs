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
        protected abstract Wall GetHostElement();
        protected abstract FamilyInstance UpdateParamsWindowGap(FamilyInstance windowGap);
        
        public FamilyInstance PlaceWindowGap(Document document, FamilySymbol windowGapType) {
            XYZ location = GetLocation();
            Element hostElement = GetNearestElement();
            
            FamilyInstance windowGap = document.Create.NewFamilyInstance(location, windowGapType, hostElement, StructuralType.NonStructural);
            return UpdateParamsWindowGap(windowGap);
        }

        private Element GetNearestElement() {
            var hostElement = GetHostElement();
            return _revitRepository.GetNearestElement(hostElement);
        }
    }
}