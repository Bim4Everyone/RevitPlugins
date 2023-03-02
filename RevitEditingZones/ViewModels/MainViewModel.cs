using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitEditingZones.Models;

namespace RevitEditingZones.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private string _errorText;
        private ObservableCollection<LevelViewModel> _levels;
        
        private ZonePlansViewModel _leftZonePlans;
        private ZonePlansViewModel _rightZonePlans;

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            LoadViewCommand = new RelayCommand(LoadView);
        }

        public ICommand LoadViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public ObservableCollection<LevelViewModel> Levels {
            get => _levels;
            set => this.RaiseAndSetIfChanged(ref _levels, value);
        }

        public ZonePlansViewModel LeftZonePlans {
            get => _leftZonePlans;
            set => this.RaiseAndSetIfChanged(ref _leftZonePlans, value);
        }
        
        public ZonePlansViewModel RightZonePlans {
            get => _rightZonePlans;
            set => this.RaiseAndSetIfChanged(ref _rightZonePlans, value);
        }

        private void LoadView(object p) {
            var levels = _revitRepository.GetLevels()
                .Select(item => new LevelViewModel(item));
            Levels = new ObservableCollection<LevelViewModel>(levels);

            LeftZonePlans = new ZonePlansViewModel();
            RightZonePlans = new ZonePlansViewModel();
            
            LeftZonePlans.ZonePlans = new ObservableCollection<ZonePlanViewModel>();
            RightZonePlans.ZonePlans = new ObservableCollection<ZonePlanViewModel>();
            foreach(ViewPlan areaPlane in _revitRepository.GetAreaPlanes()) {
                foreach(Area area in _revitRepository.GetAreas(areaPlane)) {
                    var level = RemoveLevel(_revitRepository.GetLevel(area));
                    var zonePlan = new ZonePlanViewModel(area, areaPlane) {Level = level, Levels = Levels};
                    zonePlan.ErrorType = GetErrorType(zonePlan);
                    if(zonePlan.ErrorType == ErrorType.Default) {
                        RightZonePlans.ZonePlans.Add(zonePlan);
                    } else {
                        LeftZonePlans.ZonePlans.Add(zonePlan);
                    }
                }
            }
        }

        private LevelViewModel RemoveLevel(Level level) {
            if(level == null) {
                return null;
            }

            var foundLevel = Levels.FirstOrDefault(item => item.Level.Id == level.Id);
            Levels.Remove(foundLevel);
            return foundLevel;
        }

        private ErrorType GetErrorType(ZonePlanViewModel zonePlan) {
            if(zonePlan.IsNotLinkedZones()) {
                return ErrorType.NotLinkedZones;
            }

            if(zonePlan.IsZoneNotMatchViewPlan()) {
                return ErrorType.ZoneNotMatchViewPlan;
            }

            if(zonePlan.IsZoneMatchWithSameLevels(LeftZonePlans.ZonePlans)) {
                return ErrorType.ZoneMatchWithSameLevels;
            }

            if(zonePlan.IsZoneNotMatchNames()) {
                return ErrorType.ZoneNotMatchNames;
            }

            return ErrorType.Default;
        }
    }
}