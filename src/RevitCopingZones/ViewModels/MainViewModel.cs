using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCopingZones.Models;

namespace RevitCopingZones.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly CopingZonesConfig _copingZonesConfig;

        private string _errorText;
        private Area[] _selectedAreas;
        private FloorPlanViewModel _floorPlan;
        private ObservableCollection<FloorPlanViewModel> _floorPlans;
        private string _countOfAreas;

        public MainViewModel(RevitRepository revitRepository, CopingZonesConfig copingZonesConfig) {
            _revitRepository = revitRepository;
            _copingZonesConfig = copingZonesConfig;

            LoadViewCommand = new RelayCommand(LoadView);
            SelectAreasCommand = new RelayCommand(SelectAreas);
            ExecuteViewCommand = new RelayCommand(ExecuteView, CanExecuteView);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand SelectAreasCommand { get; }
        public ICommand ExecuteViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string CountOfAreas {
            get => _countOfAreas;
            set => this.RaiseAndSetIfChanged(ref _countOfAreas, value);
        }

        public FloorPlanViewModel FloorPlan {
            get => _floorPlan;
            set => this.RaiseAndSetIfChanged(ref _floorPlan, value);
        }

        public ObservableCollection<FloorPlanViewModel> FloorPlans {
            get => _floorPlans;
            set => this.RaiseAndSetIfChanged(ref _floorPlans, value);
        }

        private void LoadView(object p) {
            var floorPlans = _revitRepository.GetFloorPlans()
                .Select(item => new FloorPlanViewModel(item))
                .OrderBy(item=> item.FloorPlan.Elevation);
            FloorPlans = new ObservableCollection<FloorPlanViewModel>(floorPlans);
            
            _selectedAreas = _revitRepository.GetSelectedAreas().ToArray();
            UpdateCountOfAreas();
        }

        private void SelectAreas(object p) {
            _selectedAreas = _revitRepository.SelectedAreas().ToArray();
            UpdateCountOfAreas();
        }

        private void ExecuteView(object p) {
            var floorPlans = FloorPlans
                .Where(item => item.IsSelected)
                .Where(item => item.CanCopyAreas)
                .Select(item => item.FloorPlan);

            using(Transaction transaction = _revitRepository.StartTransaction("Копирование зон")) {
                foreach(FloorPlan floorPlan in floorPlans) {
                    var copiedAreas = _revitRepository.CopyAreaToView(floorPlan, _selectedAreas);
                    foreach(Area copiedArea in copiedAreas) {
                        _revitRepository.UpdateAreaName(copiedArea, floorPlan);
                    }
                }

                transaction.Commit();
            }
        }

        private bool CanExecuteView(object p) {
            if(_selectedAreas == null
               || _selectedAreas.Length == 0) {
                ErrorText = "Выберите зоны на плане.";
                return false;
            }

            if(!FloorPlans.Any(item => item.IsSelected && item.CanCopyAreas)) {
                ErrorText = "Выберите этажи на которые разрешено копирование.";
                return false;
            }

            ErrorText = null;
            return true;
        }
        
        private void UpdateCountOfAreas() {
            CountOfAreas = _selectedAreas.Length == 0
                ? null
                : $"Выбрано зон: \"{_selectedAreas.Length} шт.\"";
        }
    }
}