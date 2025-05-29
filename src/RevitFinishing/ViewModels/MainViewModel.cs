using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Ninject;

using RevitFinishing.Models;
using RevitFinishing.Models.Finishing;
using RevitFinishing.Views;

namespace RevitFinishing.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private readonly List<Phase> _phases;
    private Phase _selectedPhase;

    private ObservableCollection<RoomGroupViewModel> _rooms;

    private string _errorText;

    private readonly IKernel _kernel;
    
    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
    public MainViewModel(PluginConfig pluginConfig,
                         RevitRepository revitRepository,
                         ILocalizationService localizationService, IKernel kernel) {        
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;

        _kernel = kernel;

        ProjectSettingsLoader settings = new ProjectSettingsLoader(_revitRepository.Application,
                                                           _revitRepository.Document);

        settings.CopyKeySchedule();
        settings.CopyParameters();

        _phases = _revitRepository.GetPhases();
        SelectedPhase = _phases[_phases.Count - 1];

        CalculateFinishingCommand = RelayCommand.Create(CalculateFinishing, CanCalculateFinishing);
        CheckAllCommand = RelayCommand.Create(CheckAll);
        UnCheckAllCommand = RelayCommand.Create(UnCheckAll);
        InvertAllCommand = RelayCommand.Create(InvertAll);

        LoadConfig();
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

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    private void CalculateFinishing() {
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
            var window = _kernel.Get<ErrorsWindow>();
            window.DataContext = mainErrors;
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
            var window = _kernel.Get<ErrorsWindow>();
            window.DataContext = mainErrors;
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
            var window = _kernel.Get<ErrorsWindow>();
            window.DataContext = warnings;
            window.Show();
        }

        using(Transaction t = _revitRepository.Document.StartTransaction("Заполнить параметры отделки")) {
            foreach(var element in calculator.FinishingElements) {
                element.UpdateFinishingParameters();
                element.UpdateCategoryParameters();
            }
            t.Commit();
        }

        SaveConfig();
    }

    private bool CanCalculateFinishing() {
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

    private void CheckAll() {
        _revitRepository.SetAll(Rooms, true);
    }

    private void UnCheckAll() {
        _revitRepository.SetAll(Rooms, false);
    }

    private void InvertAll() {
        _revitRepository.InvertAll(Rooms);
    }

    private void LoadConfig() {
        var settings = _pluginConfig.GetSettings(_revitRepository.Document);

        if(settings is null) {
            settings = _pluginConfig.AddSettings(_revitRepository.Document);
        }

        settings.Phase = SelectedPhase.Name;
        settings.RoomNames = Rooms
            .Where(x => x.IsChecked)
            .Select(x => x.Name)
            .ToList();

        _pluginConfig.SaveProjectConfig();
    }

    private void SaveConfig() {
        var settings = _pluginConfig.GetSettings(_revitRepository.Document);

        if(settings is null) {
            settings = _pluginConfig.AddSettings(_revitRepository.Document);
        }

        settings.Phase = SelectedPhase.Name;
        settings.RoomNames = Rooms
            .Where(x => x.IsChecked)
            .Select(x => x.Name)
            .ToList();

        _pluginConfig.SaveProjectConfig();
    }
}
