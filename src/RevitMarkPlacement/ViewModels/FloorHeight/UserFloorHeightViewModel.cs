
using dosymep.WPF.ViewModels;

using RevitMarkPlacement.Models;

namespace RevitMarkPlacement.ViewModels {
    internal class UserFloorHeightViewModel : BaseViewModel, IFloorHeightProvider {
        private string _floorHeight;

        public UserFloorHeightViewModel(string description, AnnotationsSettings settings) {
            Description = description;
            FloorHeight = settings.LevelHeight.ToString();
        }
        public string Description { get; }
        public string FloorHeight { 
            get => _floorHeight; 
            set => this.RaiseAndSetIfChanged(ref _floorHeight, value); 
        }

        public bool IsEnabled => true;

        public string GetFloorHeight() {
            return FloorHeight;
        }
    }
}
