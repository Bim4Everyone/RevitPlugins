
using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitMarkPlacement.ViewModels {
    internal class SpotDimensionTypeViewModel : BaseViewModel {
        private readonly SpotDimensionType _spotDimensionType;

        public SpotDimensionTypeViewModel(SpotDimensionType spotDimensionType) {
            _spotDimensionType = spotDimensionType;
        }

        public string Name => _spotDimensionType.Name;
    }
}
