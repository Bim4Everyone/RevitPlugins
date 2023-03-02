using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

namespace RevitEditingZones.ViewModels {
    internal class ZonePlansViewModel : BaseViewModel {
        private ZonePlanViewModel _zonePlan;
        private ObservableCollection<ZonePlanViewModel> _zonePlans;
        
        public ZonePlanViewModel ZonePlan {
            get => _zonePlan;
            set => this.RaiseAndSetIfChanged(ref _zonePlan, value);
        }

        public ObservableCollection<ZonePlanViewModel> ZonePlans {
            get => _zonePlans;
            set => this.RaiseAndSetIfChanged(ref _zonePlans, value);
        }
    }
}