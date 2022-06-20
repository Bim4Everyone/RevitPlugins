using Autodesk.Revit.DB;

using RevitWindowGapPlacement.Model.LocationProviders;

namespace RevitWindowGapPlacement.Model {
    internal abstract class BaseElement {
        protected readonly Element _element;
        protected readonly RevitRepository _revitRepository;

        public BaseElement(Element element, RevitRepository revitRepository) {
            _element = element;
            _revitRepository = revitRepository;
        }
        
        public abstract double Width { get; }
        public abstract double Height { get; }

        public Element Element => _element;
        public RevitRepository RevitRepository => _revitRepository;
        
        public ILocationProvider LocationProvider { get; set; }

        public XYZ Location => LocationProvider.GetLocation(this);
        public XYZ PlaceLocation => LocationProvider.GetPlaceLocation(this);
        public XYZ CenterLocation => LocationProvider.GetCenterLocation(this);

        protected abstract Element GetHostObject();
        protected abstract HostObject GetNextHostObject();
    }
}