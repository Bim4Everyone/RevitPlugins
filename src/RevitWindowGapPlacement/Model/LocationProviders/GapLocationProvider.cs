using Autodesk.Revit.DB;

namespace RevitWindowGapPlacement.Model.LocationProviders {
    internal class GapLocationProvider : ILocationProvider {
        public XYZ GetLocation(BaseElement baseElement) {
            return ((LocationPoint) baseElement.Element.Location).Point;
        }

        public XYZ GetPlaceLocation(BaseElement baseElement) {
            throw new System.NotImplementedException();
        }

        public XYZ GetCenterLocation(BaseElement baseElement) {
            throw new System.NotImplementedException();
        }
    }
}