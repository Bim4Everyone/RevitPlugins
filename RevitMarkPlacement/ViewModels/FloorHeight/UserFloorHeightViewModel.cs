
using dosymep.WPF.ViewModels;

namespace RevitMarkPlacement.ViewModels {
    internal class UserFloorHeightViewModel : BaseViewModel, IFloorHeightProvider {
        private string _floorHeight;

        public UserFloorHeightViewModel(string description) {
            Description = description;
        }
        public string Description { get; }
        public string FloorHeight { 
            get => _floorHeight; 
            set => this.RaiseAndSetIfChanged(ref _floorHeight, value); 
        }

        public string GetFloorHeight() {
            return FloorHeight;
        }
    }
}
