using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitWindowGapPlacement.Model {
    internal class ParentCurtainWindow : ParentWindow {
        private readonly Wall _curtainWall;

        public ParentCurtainWindow(Wall curtainWall, RevitRepository revitRepository)
            : base(curtainWall, revitRepository) {
            _curtainWall = curtainWall;
        }

        public override double Width 
            => _curtainWall.GetParamValue<double>(BuiltInParameter.CURVE_ELEM_LENGTH);
        
        public override double Height 
            => _curtainWall.GetParamValue<double>(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
        
        protected override Element GetHostObject() {
            return _curtainWall;
        }

        protected override HostObject GetNextHostObject() {
            throw new System.NotImplementedException();
        }

        protected override IEnumerable<HostObject> GetHostElements() {
            return _revitRepository.GetNearestElements(_curtainWall, PlaceLocation - new XYZ(0, 0, Height / 2), _curtainWall.Orientation);
        }

        protected override FamilyInstance UpdateParamsWindowGap(FamilyInstance windowGap) {
            windowGap.SetParamValue(BuiltInParameter.WINDOW_WIDTH, Width);
            windowGap.SetParamValue(BuiltInParameter.WINDOW_HEIGHT, Height);
            
            return windowGap;
        }
    }
}