
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.RevitViewSettings {
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
