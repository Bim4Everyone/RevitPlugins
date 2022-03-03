
using dosymep.WPF.ViewModels;

namespace RevitMarkPlacement.ViewModels {
    internal class UserFloorHeightViewModel : BaseViewModel, IFloorHeightProvider {
        private double _floorHeight;

        public UserFloorHeightViewModel(string description) {
            Description = description;
        }
        public string Description { get; }
        public double FloorHeight { 
            get => _floorHeight; 
            set => this.RaiseAndSetIfChanged(ref _floorHeight, value); 
        }

        public double GetFloorHeight() {
            return FloorHeight;
        }
    }
}
