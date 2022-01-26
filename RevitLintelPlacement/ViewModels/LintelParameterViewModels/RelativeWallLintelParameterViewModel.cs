
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class RelativeWallLintelParameterViewModel : BaseViewModel, ILintelParameterViewModel {
        private double _relationValue;
        private string _name;
        private string _wallParameterName;

        public double RelationValue {
            get => _relationValue;
            set => this.RaiseAndSetIfChanged(ref _relationValue, value);
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string WallParameterName {
            get => _wallParameterName;
            set => this.RaiseAndSetIfChanged(ref _wallParameterName, value);
        }
    }


}
