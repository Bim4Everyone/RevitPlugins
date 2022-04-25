using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitWindowGapPlacement.Model {
    internal class ParentCurtainWallWindow : ParentWindow {
        private readonly Wall _curtainWall;

        public ParentCurtainWallWindow(Wall curtainWall, RevitRepository revitRepository)
            : base(curtainWall, revitRepository) {
            _curtainWall = curtainWall;
        }

        protected override XYZ GetPlaceLocation() {
            return _revitRepository.GetPlaceLocation(_curtainWall);
        }

        protected override Element GetHostObject() {
            return _curtainWall;
        }

        protected override HostObject GetNextHostObject() {
            throw new System.NotImplementedException();
        }

        protected override IEnumerable<HostObject> GetHostElements() {
            XYZ point = GetPlaceLocation();
            double height = _curtainWall.GetParamValue<double>(BuiltInParameter.WALL_USER_HEIGHT_PARAM) / 2;
            return _revitRepository.GetNearestElements(_curtainWall, point - new XYZ(0, 0, height), _curtainWall.Orientation);
        }

        protected override FamilyInstance UpdateParamsWindowGap(FamilyInstance windowGap) {
            double width = _curtainWall.GetParamValue<double>(BuiltInParameter.CURVE_ELEM_LENGTH);
            double height = _curtainWall.GetParamValue<double>(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
            
            windowGap.SetParamValue(BuiltInParameter.WINDOW_WIDTH, width);
            windowGap.SetParamValue(BuiltInParameter.WINDOW_HEIGHT, height);
            
            return windowGap;
        }
    }
}