using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitEditingZones.Models;
using RevitEditingZones.Services;

namespace RevitEditingZones.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly ILevelsWindowService _levelWindowService;

        private string _errorText;
        private ObservableCollection<LevelViewModel> _levels;
        private ObservableCollection<LevelViewModel> _allLevels;
        
        private ZonePlansViewModel _leftZonePlans;
        private ZonePlansViewModel _rightZonePlans;

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository, ILevelsWindowService levelWindowService) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _levelWindowService = levelWindowService;

            ViewCommand = new RelayCommand(UpdateLinks);
            LoadViewCommand = new RelayCommand(LoadView);
            ShowLevelsCommand = new RelayCommand(ShowLevels);
        }

        public ICommand ViewCommand { get; }
        public ICommand LoadViewCommand { get; }
        public ICommand ShowLevelsCommand { get; }
        
        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public ObservableCollection<LevelViewModel> Levels {
            get => _levels;
            set => this.RaiseAndSetIfChanged(ref _levels, value);
        }
        
        public ObservableCollection<LevelViewModel> AllLevels {
            get => _allLevels;
            set => this.RaiseAndSetIfChanged(ref _allLevels, value);
        }

        public ZonePlansViewModel LeftZonePlans {
            get => _leftZonePlans;
            set => this.RaiseAndSetIfChanged(ref _leftZonePlans, value);
        }
        
        public ZonePlansViewModel RightZonePlans {
            get => _rightZonePlans;
            set => this.RaiseAndSetIfChanged(ref _rightZonePlans, value);
        }

        public IEnumerable<ZonePlanViewModel> ZonePlans => LeftZonePlans.ZonePlans.Union(RightZonePlans.ZonePlans);

        private void LoadView(object p) {
            var levels = _revitRepository.GetLevels()
                .Select(item => new LevelViewModel(item))
                .ToList();
            
            Levels = new ObservableCollection<LevelViewModel>(levels);
            AllLevels = new ObservableCollection<LevelViewModel>(levels);
            
            LeftZonePlans = new ZonePlansViewModel() {HintText = "Ошибки не найдены"};
            RightZonePlans = new ZonePlansViewModel() {HintText = "Настроенные зоны не найдены"};

            LeftZonePlans.ZonePlans = new ObservableCollection<ZonePlanViewModel>();
            RightZonePlans.ZonePlans = new ObservableCollection<ZonePlanViewModel>();
            
            foreach(ViewPlan areaPlane in _revitRepository.GetAreaPlanes()) {
                foreach(Area area in _revitRepository.GetAreas(areaPlane)) {
                    var level = GetLevel(area);
                    Levels.Remove(level);
                    
                    var zonePlan = new ZonePlanViewModel(area, areaPlane) {Level = level, Levels = Levels};
                    zonePlan.ErrorType = GetErrorType(zonePlan);
                    if(zonePlan.ErrorType == ErrorType.Default) {
                        RightZonePlans.ZonePlans.Add(zonePlan);
                    } else {
                        LeftZonePlans.ZonePlans.Add(zonePlan);
                    }
                }
            }

            LeftZonePlans.HintText = Levels.Count == 0
                ? "Ошибки не найдены"
                : "Ошибки в зонах не найдены. Имеются уровни без зон!";
        }

        private void UpdateLinks(object obj) {
            using(Transaction transaction = _revitRepository.StartTransaction("Обновление привязок")) {
                foreach(ZonePlanViewModel zonePlan in ZonePlans) {
                    _revitRepository.UpdateAreaName(zonePlan.Area, zonePlan.AreaName);
                    _revitRepository.UpdateAreaLevel(zonePlan.Area, zonePlan.Level?.Level);
                }

                transaction.Commit();
            }
            
            GetPlatformService<INotificationService>()
                .CreateNotification("Редактор зон СМР ", "Обновление привязок завершено.", "C#")
                .ShowAsync();
        }
        
        private void ShowLevels(object obj) {
            _levelWindowService.ShowLevels(Levels);
        }

        private LevelViewModel GetLevel(Area area) {
            var level = _revitRepository.GetLevel(area);
            return AllLevels.FirstOrDefault(item => item.Level.Id == level?.Id);
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