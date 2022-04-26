using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitWindowGapPlacement.Model.LocationProviders {
    internal class CurtainLocationProvider : ILocationProvider {
        public XYZ GetLocation(BaseElement baseElement) {
            LocationCurve location = (LocationCurve) baseElement.Element.Location;
            Line line = (Line) location.Curve;
            return line.Origin - line.GetEndPoint(0);
        }

        public XYZ GetPlaceLocation(BaseElement baseElement) {
            LocationCurve location = (LocationCurve) baseElement.Element.Location;

            Line line = (Line) location.Curve;
            double offset = baseElement.Element.GetParamValue<double>(BuiltInParameter.WALL_BASE_OFFSET);
            double height = baseElement.Element.GetParamValue<double>(BuiltInParameter.WALL_USER_HEIGHT_PARAM);

            var startPoint = line.GetEndPoint(0);
            var finishPoint = line.GetEndPoint(1);

            return line.Origin - (startPoint - finishPoint) / 2 + new XYZ(0, 0, height / 2 + offset);
        }

        public XYZ GetCenterLocation(BaseElement baseElement) {
            throw new System.NotImplementedException();
        }
    }
}