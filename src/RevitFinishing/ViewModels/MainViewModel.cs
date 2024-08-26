using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.ProjectParams;
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

            FinishingInProject allFinishing = new FinishingInProject(_revitRepository, SelectedPhase);
            FinishingChecker checker = new FinishingChecker(SelectedPhase);
            ErrorsViewModel mainErrors = new ErrorsViewModel();


            mainErrors.AddElements(new ErrorsListViewModel("Ошибка") {
                Description = "На выбранной стадии не найдены экземпляры отделки",
                ErrorElements = new ObservableCollection<ErrorElement>(
                    checker.CheckPhaseContainsFinishing(allFinishing))
            });
            mainErrors.AddElements(new ErrorsListViewModel("Ошибка") {
                Description = "Экземпляры отделки являются границами помещений",
                ErrorElements = new ObservableCollection<ErrorElement>(
                    checker.CheckFinishingByRoomBounding(allFinishing))
            });
            string finishingKeyParam = ProjectParamsConfig.Instance.RoomFinishingType.Name;
            mainErrors.AddElements(new ErrorsListViewModel("Ошибка") {
                Description = "У помещений не заполнен ключевой параметр отделки",
                ErrorElements = new ObservableCollection<ErrorElement>(
                    checker.CheckRoomsByKeyParameter(selectedRooms, finishingKeyParam))
            });
            if(mainErrors.ErrorLists.Any()) {
                var window = new ErrorsWindow(mainErrors);
                window.Show();
                return;
            }

            FinishingCalculator calculator = new FinishingCalculator(selectedRooms, allFinishing);
            ErrorsViewModel otherErrors = new ErrorsViewModel();
            ErrorsViewModel warnings = new ErrorsViewModel();

            otherErrors.AddElements(new ErrorsListViewModel("Ошибка") {
                Description = "Элементы отделки относятся к помещениям с разными типами отделки",
                ErrorElements = new ObservableCollection<ErrorElement>(
                    checker.CheckFinishingByRoom(calculator.FinishingElements))
            });
            if(otherErrors.ErrorLists.Any()) {
                var window = new ErrorsWindow(otherErrors);
                window.Show();
                return;
            }

            string numberParamName = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_NUMBER);
            warnings.AddElements(new ErrorsListViewModel("Предупреждение") {
                Description = $"У помещений не заполнен параметр \"{numberParamName}\"",
                ErrorElements = new ObservableCollection<ErrorElement>(
                    checker.CheckRoomsByParameter(selectedRooms, numberParamName))
            });
            string nameParamName = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_NAME);
            warnings.AddElements(new ErrorsListViewModel("Предупреждение") {
                Description = $"У помещений не заполнен параметр \"{nameParamName}\"",
                ErrorElements = new ObservableCollection<ErrorElement>(
                    checker.CheckRoomsByParameter(selectedRooms, nameParamName))
            });
            if(warnings.ErrorLists.Any()) {
                var window = new ErrorsWindow(warnings);
                window.Show();
            }

            using(Transaction t = _revitRepository.Document.StartTransaction("Заполнить параметры отделки")) {
                foreach(var element in calculator.FinishingElements) {
                    element.UpdateFinishingParameters();
                    element.UpdateCategoryParameters();
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
