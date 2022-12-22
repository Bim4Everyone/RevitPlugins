
using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RevitViews.RevitViewSettings {
    internal class DetailLevelSetting : IView3DSetting {
        private readonly ViewDetailLevel _detailLevel;

        public DetailLevelSetting(ViewDetailLevel detailLevel) {
            _detailLevel = detailLevel;
        }

        public void Apply(View3D view3D) {
            if(view3D.CanModifyDetailLevel()) {
                view3D.DetailLevel = _detailLevel;
            }
        }
    }
}
