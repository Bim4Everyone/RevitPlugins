using Autodesk.Revit.DB;

namespace RevitWindowGapPlacement.Model.LocationProviders {
    internal interface ILocationProvider {
        XYZ GetLocation(BaseElement baseElement);
        XYZ GetPlaceLocation(BaseElement baseElement);
        XYZ GetCenterLocation(BaseElement baseElement);
    }
}