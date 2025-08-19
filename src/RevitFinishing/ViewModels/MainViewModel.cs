using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitFinishing.Models;
using RevitFinishing.Models.Finishing;
using RevitFinishing.Services;
using RevitFinishing.ViewModels.Notices;

namespace RevitFinishing.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly FinishingInProject _finishingInProject;
    private readonly ErrorWindowService _errorWindowService;
    private readonly ProjectValidationService _projectValidationService;

    private readonly IList<Phase> _phases;
    private Phase _selectedPhase;

    private ObservableCollection<RoomNameVM> _roomNames;
    private ObservableCollection<RoomDepartmentVM> _roomDepartments;
    private ObservableCollection<RoomLevelVM> _roomLevels;

    private string _errorText;

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
    public MainViewModel(PluginConfig pluginConfig,
                         RevitRepository revitRepository,
                         ILocalizationService localizationService,
                         FinishingInProject finishingInProject,
                         ProjectValidationService projectValidationService,
                         ErrorWindowService errorWindowService) {
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _finishingInProject = finishingInProject;
        _errorWindowService = errorWindowService;
        _projectValidationService = projectValidationService;

        var settings = new ProjectSettingsLoader(_revitRepository.Application, _revitRepository.Document);

        settings.CopyKeySchedule();
        settings.CopyParameters();

        _phases = _revitRepository.GetPhases();
        SelectedPhase = _phases[_phases.Count - 1];

        CalculateFinishingCommand = RelayCommand.Create(CalculateFinishing, CanCalculateFinishing);

        LoadConfig();
    }

    public ICommand CalculateFinishingCommand { get; }

    public IList<Phase> Phases => _phases;

    public Phase SelectedPhase {
        get => _selectedPhase;
        set {
            RaiseAndSetIfChanged(ref _selectedPhase, value);

            RoomNames = [.. _revitRepository
                .GetParamStringValues(_selectedPhase, BuiltInParameter.ROOM_NAME)
                .Select(x => new RoomNameVM(x, BuiltInParameter.ROOM_NAME, _localizationService))
                .OrderBy(x => x.Name)];

            RoomDepartments = [.. _revitRepository
                .GetParamStringValues(_selectedPhase, BuiltInParameter.ROOM_DEPARTMENT)
                .Select(x => new RoomDepartmentVM(x, BuiltInParameter.ROOM_DEPARTMENT, _localizationService))
                .OrderBy(x => x.Name)];

            RoomLevels = [.. _revitRepository
                .GetRoomLevels(_selectedPhase)
                .Select(x => new RoomLevelVM(x, x.Name, BuiltInParameter.ROOM_UPPER_LEVEL, _localizationService))
                .OrderBy(x => x.Name)];
        }
    }

    public ObservableCollection<RoomNameVM> RoomNames {
        get => _roomNames;
        set => RaiseAndSetIfChanged(ref _roomNames, value);
    }

    public ObservableCollection<RoomDepartmentVM> RoomDepartments {
        get => _roomDepartments;
        set => RaiseAndSetIfChanged(ref _roomDepartments, value);
    }

    public ObservableCollection<RoomLevelVM> RoomLevels {
        get => _roomLevels;
        set => RaiseAndSetIfChanged(ref _roomLevels, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    private void CalculateFinishing() {
        IEnumerable<RoomNameVM> selectedRoomNames = RoomNames.Where(x => x.IsChecked);
        IEnumerable<RoomDepartmentVM> selectedRoomDepartments = RoomDepartments.Where(x => x.IsChecked);
        IEnumerable<RoomLevelVM> selectedRoomLevels = RoomLevels.Where(x => x.IsChecked);

        IList<ElementFilter> orFilters = [];

        orFilters = GetLogicalFilter(orFilters, selectedRoomNames);
        orFilters = GetLogicalFilter(orFilters, selectedRoomDepartments);
        orFilters = GetLogicalFilter(orFilters, selectedRoomLevels);

        IList<Room> selectedRooms = _revitRepository.GetRoomsByFilters(orFilters);

        _finishingInProject.CalculateAllFinishing(_revitRepository, SelectedPhase);

        ErrorsViewModel mainErrors = _projectValidationService
            .CheckMainErrors(_finishingInProject, selectedRooms, _selectedPhase);
        if(_errorWindowService.ShowNoticeWindow(mainErrors)) {
            return;
        }

        var calculator = new FinishingCalculator(selectedRooms, _finishingInProject);

        ErrorsViewModel finishingErrors = _projectValidationService
            .CheckFinishingErrors(calculator, _selectedPhase);
        if(_errorWindowService.ShowNoticeWindow(finishingErrors)) {
            return;
        }

        IEnumerable<FinishingElement> finishingElements = calculator.FinishingElements;

        string transactionName = _localizationService.GetLocalizedString("MainWindow.TransactionName");
        using(Transaction t = _revitRepository.Document.StartTransaction(transactionName)) {
            foreach(FinishingElement element in finishingElements) {
                element.ClearFinishingParameters();
                element.UpdateFinishingParameters(calculator);
                element.UpdateCategoryParameters(calculator);
            }
            t.Commit();
        }

        WarningsViewModel parameterErrors = _projectValidationService
            .CheckWarnings(selectedRooms, finishingElements, _selectedPhase, _revitRepository.Document);
        _errorWindowService.ShowNoticeWindow(parameterErrors);

        SaveConfig();
    }

    private bool CanCalculateFinishing() {
        if(!RoomNames.Any()) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoRooms");
            return false;
        }
        if(!RoomNames.Any(x => x.IsChecked)
            && !RoomDepartments.Any(x => x.IsChecked)
            && !RoomLevels.Any(x => x.IsChecked)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoSelectiom");
            return false;
        }

        ErrorText = "";
        return true;
    }

    private IList<ElementFilter> GetLogicalFilter(IList<ElementFilter> filter,
                                                  IEnumerable<SelectionElementVM> elements) {
        if(elements.Any()) {
            List<ElementFilter> levelParamFilters = [];
            foreach(SelectionElementVM roomLevel in elements) {
                levelParamFilters.Add(roomLevel.GetParameterFilter());
            }
            filter.Add(new LogicalOrFilter(levelParamFilters));
        }
        return filter;
    }

    private void LoadConfig() {
        RevitSettings settings = _pluginConfig.GetSettings(_revitRepository.Document);

        settings ??= _pluginConfig.AddSettings(_revitRepository.Document);

        SelectedPhase = Phases.FirstOrDefault(x => x.Name == settings.Phase) ?? _phases[_phases.Count - 1];

        var rooms = RoomNames.Where(x => settings.RoomNames.Contains(x.Name));
        var departments = RoomDepartments.Where(x => settings.RoomDepartments.Contains(x.Name));
        var levels = RoomLevels.Where(x => settings.RoomLevels.Contains(x.Name));

        foreach(var room in rooms) {
            room.IsChecked = true;
        }

        foreach(var department in departments) {
            department.IsChecked = true;
        }

        foreach(var level in levels) {
            level.IsChecked = true;
        }

        _pluginConfig.SaveProjectConfig();
    }

    private void SaveConfig() {
        RevitSettings settings = _pluginConfig.GetSettings(_revitRepository.Document);

        settings ??= _pluginConfig.AddSettings(_revitRepository.Document);

        settings.Phase = SelectedPhase.Name;
        settings.RoomNames = [.. RoomNames
            .Where(x => x.IsChecked)
            .Select(x => x.Name)];
        
        settings.RoomDepartments = [.. RoomDepartments
            .Where(x => x.IsChecked)
            .Select(x => x.Name)];

        settings.RoomLevels = [.. RoomLevels
            .Where(x => x.IsChecked)
            .Select(x => x.Name)];

        _pluginConfig.SaveProjectConfig();
    }
}
