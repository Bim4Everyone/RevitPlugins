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
        
        private ZonePlanViewModel _zonePlan;
        private ObservableCollection<ZonePlanViewModel> _zonePlans;

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
        
        public ZonePlanViewModel ZonePlan {
            get => _zonePlan;
            set => this.RaiseAndSetIfChanged(ref _zonePlan, value);
        }

        public ObservableCollection<ZonePlanViewModel> ZonePlans {
            get => _zonePlans;
            set => this.RaiseAndSetIfChanged(ref _zonePlans, value);
        }

        private void LoadView(object p) {
            var levels = _revitRepository.GetLevels()
                .Select(item => new LevelViewModel(item));
            Levels = new ObservableCollection<LevelViewModel>(levels);

            ZonePlans = new ObservableCollection<ZonePlanViewModel>();
            foreach(ViewPlan areaPlane in _revitRepository.GetAreaPlanes()) {
                foreach(Area area in _revitRepository.GetAreas(areaPlane)) {
                    var level = RemoveLevel(_revitRepository.GetLevel(area));
                    ZonePlans.Add(new ZonePlanViewModel(area, areaPlane) {Level = level, Levels = Levels});
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
    }
}