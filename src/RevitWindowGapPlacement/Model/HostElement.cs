using Autodesk.Revit.DB;

namespace RevitWindowGapPlacement.Model {
    internal class HostElement {
        private readonly HostObject _hostObject;
        private readonly RevitRepository _revitRepository;

        public HostElement(HostObject element, RevitRepository revitRepository) {
            _hostObject = element;
            _revitRepository = revitRepository;
        }
    }
}