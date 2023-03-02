using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitEditingZones.ViewModels {
    internal class ZonePlansViewModel : BaseViewModel {
        private ZonePlanViewModel _zonePlan;
        private ObservableCollection<ZonePlanViewModel> _zonePlans;

        public ZonePlansViewModel() {
            AutoLinkZonesCommand = new RelayCommand(AutoLinkZones);
            UpdateZoneNamesCommand = new RelayCommand(UpdateZoneNames);
        }

        public ICommand AutoLinkZonesCommand { get; }
        public ICommand UpdateZoneNamesCommand { get; }

        public ZonePlanViewModel ZonePlan {
            get => _zonePlan;
            set => this.RaiseAndSetIfChanged(ref _zonePlan, value);
        }

        public ObservableCollection<ZonePlanViewModel> ZonePlans {
            get => _zonePlans;
            set => this.RaiseAndSetIfChanged(ref _zonePlans, value);
        }

        private void AutoLinkZones(object obj) {
            if(obj is object[] zonePlans) {
                foreach(ZonePlanViewModel zonePlan in zonePlans.OfType<ZonePlanViewModel>()) {
                    var levels = zonePlan.Levels
                        .Where(item => item.LevelName.Equals(zonePlan.AreaName))
                        .ToArray();

                    if(levels.Length == 1) {
                        zonePlan.Level = levels[0];
                    }
                }
            }
        }

        private void UpdateZoneNames(object obj) {
            if(obj is object[] zonePlans) {
                foreach(ZonePlanViewModel zonePlan in zonePlans.OfType<ZonePlanViewModel>()) {
                    zonePlan.AreaName = zonePlan.Level.LevelName;
                }
            }
        }
    }
}