using System;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RevitViews.RevitViewSettings {
    internal class TransparencySetting : IView3DSetting {
        private readonly int _trancparencyValue;

        public TransparencySetting(int trancparencyValue) {
            _trancparencyValue = trancparencyValue;
        }
        public void Apply(View3D view3D) {
            var vdm = view3D.GetViewDisplayModel();
            vdm.Transparency = _trancparencyValue;
            view3D.SetViewDisplayModel(vdm);
        }
    }
}
