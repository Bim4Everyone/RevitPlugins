using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.RevitViewSettings {
    internal class TransparencySetting : IView3DSetting {
        private readonly int _trancparencyValue;

        public TransparencySetting(int trancparencyValue) {
            _trancparencyValue = trancparencyValue;
        }
        public void Apply(View3D view3D) {
#if REVIT_2021_OR_GREATER
            var vdm = view3D.GetViewDisplayModel();
            vdm.Transparency = _trancparencyValue;
            // в 2020 версии метод SetViewDisplayModel выбрасывает исключение ArgumentException
            view3D.SetViewDisplayModel(vdm);
#endif
        }
    }
}
