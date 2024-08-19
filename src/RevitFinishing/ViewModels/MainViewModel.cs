using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitFinishing.Models;
using RevitFinishing.Views;

namespace RevitFinishing.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private readonly List<Phase> _phases;
        private Phase _selectedPhase;

        private ObservableCollection<RoomGroupViewModel> _rooms;

        private string _errorText;

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            ProjectSettingsLoader settings = new ProjectSettingsLoader(_revitRepository.Application,
                                                                       _revitRepository.Document);
            settings.CopyKeySchedule();
            settings.CopyParameters();

            _phases = _revitRepository.GetPhases();
            SelectedPhase = _phases[_phases.Count - 1];

            CalculateFinishingCommand = new RelayCommand(CalculateFinishing, CanCalculateFinishing);
            CheckAllCommand = new RelayCommand(CheckAll);
            UnCheckAllCommand = new RelayCommand(UnCheckAll);
            InvertAllCommand = new RelayCommand(InvertAll);
        }
        public ICommand CalculateFinishingCommand { get; }
        public ICommand CheckAllCommand { get; }
        public ICommand UnCheckAllCommand { get; }
        public ICommand InvertAllCommand { get; }

        public List<Phase> Phases => _phases;

        public Phase SelectedPhase {
            get => _selectedPhase;
            set {
                RaiseAndSetIfChanged(ref _selectedPhase, value);
                _rooms = _revitRepository.GetRoomsOnPhase(_selectedPhase);
                OnPropertyChanged("Rooms");
            }
        }

        public ObservableCollection<RoomGroupViewModel> Rooms {
            get => _rooms;
            set => RaiseAndSetIfChanged(ref _rooms, value);
        }

        private void CalculateFinishing(object p) {
            IEnumerable<Element> selectedRooms = Rooms
                .Where(x => x.IsChecked)
                .SelectMany(x => x.Rooms);

            FinishingInProject finishing = new FinishingInProject(_revitRepository, SelectedPhase);
            FinishingCalculator calculator = new FinishingCalculator(selectedRooms, finishing, SelectedPhase);

            if(calculator.ErrorElements.ErrorLists.Any()) {
                var window = new ErrorsWindow() {
                    DataContext = calculator.ErrorElements
                };
                window.Show();
                return;
            }

            if(calculator.WarningElements.ErrorLists.Any()) {
                var window = new ErrorsWindow() {
                    DataContext = calculator.WarningElements
                };
                window.Show();
            }

            List<FinishingElement> finishings = calculator.Finishings;
            using(Transaction t = _revitRepository.Document.StartTransaction("Заполнить параметры отделки")) {
                foreach(var element in finishings) {
                    element.UpdateFinishingParameters();
                }
                t.Commit();
            }
        }

        private bool CanCalculateFinishing(object p) {
            if(!Rooms.Any()) {
                ErrorText = "Помещения отсутствуют на выбранной стадии";
                return false;
            }
            if(!Rooms.Any(x => x.IsChecked)) {
                ErrorText = "Помещения не выбраны";
                return false;
            }

            ErrorText = "";
            return true;
        }

        private void CheckAll(object p) {
            _revitRepository.SetAll(Rooms, true);
        }

        private void UnCheckAll(object p) {
            _revitRepository.SetAll(Rooms, false);
        }

        private void InvertAll(object p) {
            _revitRepository.InvertAll(Rooms);
        }

        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }
    }
}
