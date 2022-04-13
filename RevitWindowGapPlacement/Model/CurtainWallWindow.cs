using Autodesk.Revit.DB;

namespace RevitWindowGapPlacement.Model {
    internal class CurtainWallWindow : BaseWindow {
        private readonly Wall _wall;

        public CurtainWallWindow(Wall wall, RevitRepository revitRepository)
            : base(wall, revitRepository) {
            _wall = wall;
        }

        protected override Wall GetHostElement() {
            return _wall;
        }

        protected override XYZ GetLocation() {
            throw new System.NotImplementedException();
        }

        protected override FamilyInstance UpdateParamsWindowGap(FamilyInstance windowGap) {
            return windowGap;
        }
    }
}