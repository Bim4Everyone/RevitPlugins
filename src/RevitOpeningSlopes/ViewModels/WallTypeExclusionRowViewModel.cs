using dosymep.WPF.ViewModels;

namespace RevitOpeningSlopes.ViewModels {
    internal class WallTypeExclusionRowViewModel : BaseViewModel {
        private WallTypeExclusionViewModel _selectedWallType;

        public WallTypeExclusionRowViewModel(
            WallTypeExclusionViewModel selectedWallType) {
            _selectedWallType = selectedWallType;
        }

        public WallTypeExclusionViewModel SelectedWallType {
            get => _selectedWallType;
            set => RaiseAndSetIfChanged(ref _selectedWallType, value);
        }
    }
}
