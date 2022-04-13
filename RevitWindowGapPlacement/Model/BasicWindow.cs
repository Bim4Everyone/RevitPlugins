using Autodesk.Revit.DB;

namespace RevitWindowGapPlacement.Model {
    internal class BasicWindow : BaseWindow {
        private readonly FamilyInstance _familyInstance;

        public BasicWindow(FamilyInstance familyInstance, RevitRepository revitRepository)
            : base(familyInstance, revitRepository) {
            _familyInstance = familyInstance;
        }

        protected override Wall GetHostElement() {
            return _familyInstance.Host as Wall;
        }

        protected override XYZ GetLocation() {
            throw new System.NotImplementedException();
        }

        protected override FamilyInstance UpdateParamsWindowGap(FamilyInstance windowGap) {
            return windowGap;
        }
    }
}