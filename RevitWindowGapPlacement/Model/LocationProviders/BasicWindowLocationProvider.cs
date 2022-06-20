using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitWindowGapPlacement.Model.LocationProviders {
    internal class BasicWindowLocationProvider : ILocationProvider {
        public XYZ GetLocation(BaseElement baseElement) {
            return ((LocationPoint) baseElement.Element.Location).Point;
        }

        public XYZ GetPlaceLocation(BaseElement baseElement) {
            XYZ point = new XYZ(0, 0, baseElement.Height);

#if REVIT_2022_OR_GREATER
            var wall = (Wall) ((FamilyInstance) baseElement.Element).Host;
            if(wall.CrossSection == WallCrossSection.SingleSlanted) {
                var radian = wall.GetParamValue<double>(BuiltInParameter.WALL_SINGLE_SLANT_ANGLE_FROM_VERTICAL);

                LocationCurve location = (LocationCurve) wall.Location;
                Line line = (Line) location.Curve;

                point = Transform.CreateRotation(line.Direction, radian).OfVector(point);
            }
#endif

            return GetLocation(baseElement).Add(point);
        }

        public XYZ GetCenterLocation(BaseElement baseElement) {
            throw new System.NotImplementedException();
        }
    }
}