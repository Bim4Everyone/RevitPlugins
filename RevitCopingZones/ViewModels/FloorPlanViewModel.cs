using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitCopingZones.Models;

namespace RevitCopingZones.ViewModels {
    internal class FloorPlanViewModel : BaseViewModel {
        private readonly FloorPlan _floorPlan;
        private bool _isSelected;

        public FloorPlanViewModel(FloorPlan floorPlan) {
            _floorPlan = floorPlan;
        }

        public bool IsSelected {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public FloorPlan FloorPlan => _floorPlan;
        
        public string FloorName => _floorPlan.Name;
        public string AreaPlanName => _floorPlan.AreaPlan?.Name;
        public bool CanCopyAreas => _floorPlan.CanCopyAreas;
    }
}