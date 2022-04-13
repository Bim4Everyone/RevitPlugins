using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitWindowGapPlacement.Model {
    internal class CurtainWallWindow : BaseWindow {
        private readonly Wall _wall;

        public CurtainWallWindow(Wall wall, RevitRepository revitRepository)
            : base(wall, revitRepository) {
            _wall = wall;
        }

        protected override Wall GetHostElement() {
            XYZ point = GetPlaceLocation();
            double height = _wall.GetParamValue<double>(BuiltInParameter.WALL_USER_HEIGHT_PARAM) / 2;
            return _revitRepository.GetNearestElement(_wall, point - new XYZ(0, 0, height), _wall.Orientation);
        }

        protected override XYZ GetPlaceLocation() {
            LocationCurve location = (LocationCurve) _wall.Location;
            Line line = (Line) location.Curve;
            double height = _wall.GetParamValue<double>(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
            
            return line.Origin - ((line.GetEndPoint(0) - line.GetEndPoint(1)) / 2) + new XYZ(0, 0, height);
        }

        protected override FamilyInstance UpdateParamsWindowGap(FamilyInstance windowGap) {
            double width = _wall.GetParamValue<double>(BuiltInParameter.CURVE_ELEM_LENGTH);
            double height = _wall.GetParamValue<double>(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
            
            windowGap.SetParamValue(BuiltInParameter.WINDOW_WIDTH, width);
            windowGap.SetParamValue(BuiltInParameter.WINDOW_HEIGHT, height);
            
            return windowGap;
        }
    }
}