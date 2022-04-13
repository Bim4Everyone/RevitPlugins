using Autodesk.Revit.DB;

namespace RevitWindowGapPlacement.Model {
    internal class BasicWindow : BaseWindow {
        private readonly FamilyInstance _familyInstance;

        public BasicWindow(FamilyInstance familyInstance)
            : base(familyInstance) {
            _familyInstance = familyInstance;
        }

        protected override XYZ GetLocation() {
            throw new System.NotImplementedException();
        }

        protected override FamilyInstance UpdateParamsWindowGap(FamilyInstance windowGap) {
            return windowGap;
        }
    }
}