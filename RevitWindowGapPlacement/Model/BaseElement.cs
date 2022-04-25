using Autodesk.Revit.DB;

namespace RevitWindowGapPlacement.Model {
    internal abstract class BaseElement {
        protected readonly Element _element;
        protected readonly RevitRepository _revitRepository;

        public BaseElement(Element element, RevitRepository revitRepository) {
            _element = element;
            _revitRepository = revitRepository;
        }

        protected abstract XYZ GetPlaceLocation();
        protected abstract Element GetHostObject();
        protected abstract HostObject GetNextHostObject();
    }
}