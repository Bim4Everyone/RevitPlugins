using dosymep.WPF.ViewModels;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig {
    internal class StructureCategoryViewModel : BaseViewModel {
        private bool _isSelected;
        private string _name;

        public bool IsSelected {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
    }
}
