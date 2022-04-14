using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitWindowGapPlacement.Model {
    internal class CurtainWallWindow : BaseWindow {
        private readonly Wall _wall;

        public CurtainWallWindow(Wall wall, RevitRepository revitRepository)
            : base(wall, revitRepository) {
            _wall = wall;
        }

        protected override IEnumerable<Element> GetHostElements() {
            XYZ point = GetPlaceLocation();
            double height = _wall.GetParamValue<double>(BuiltInParameter.WALL_USER_HEIGHT_PARAM) / 2;
            return _revitRepository.GetNearestElements(_wall, point - new XYZ(0, 0, height), _wall.Orientation);
        }

        protected override XYZ GetPlaceLocation() {
            return _revitRepository.GetPlaceLocation(_wall);
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