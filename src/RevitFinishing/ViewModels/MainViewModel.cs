using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitFinishing.Models;
using RevitFinishing.Models.Finishing;
using RevitFinishing.Services;
using RevitFinishing.ViewModels.Errors;
using RevitFinishing.Views;

namespace RevitFinishing.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly ErrorWindowService _errorWindowService;
    private readonly ProjectValidationService _projectValidationService;

    private readonly List<Phase> _phases;
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
                         ProjectValidationService projectValidationService,
                         ErrorWindowService errorWindowService) {
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _errorWindowService = errorWindowService;
        _projectValidationService = projectValidationService;

        ProjectSettingsLoader settings = 
            new ProjectSettingsLoader(_revitRepository.Application, _revitRepository.Document);

        settings.CopyKeySchedule();
        settings.CopyParameters();

        _phases = _revitRepository.GetPhases();
        SelectedPhase = _phases[_phases.Count - 1];

        CalculateFinishingCommand = RelayCommand.Create(CalculateFinishing, CanCalculateFinishing);

        LoadConfig();
    }

    public ICommand CalculateFinishingCommand { get; }

    public List<Phase> Phases => _phases;

    public Phase SelectedPhase {
        get => _selectedPhase;
        set {
            RaiseAndSetIfChanged(ref _selectedPhase, value);

            RoomNames = [.. _revitRepository
                .GetParamStringValues(_selectedPhase, BuiltInParameter.ROOM_NAME)
                .Select(x => new RoomNameVM(x, BuiltInParameter.ROOM_NAME, _localizationService))];

            RoomDepartments = [.. _revitRepository
                .GetParamStringValues(_selectedPhase, BuiltInParameter.ROOM_DEPARTMENT)
                .Select(x => new RoomDepartmentVM(x, BuiltInParameter.ROOM_DEPARTMENT, _localizationService))];

            RoomLevels = [.. _revitRepository
                .GetRoomLevels(_selectedPhase)
                .Select(x => new RoomLevelVM(x, x.Name, BuiltInParameter.ROOM_UPPER_LEVEL, _localizationService))];
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
        List<Room> selectedRooms = _revitRepository.GetRoomsByFilters(
            RoomNames.Where(x => x.IsChecked), 
            RoomDepartments.Where(x => x.IsChecked), 
            RoomLevels.Where(x => x.IsChecked));

        FinishingInProject allFinishing = new FinishingInProject(_revitRepository, SelectedPhase);

        ErrorsViewModel mainErrors = _projectValidationService.CheckMainErrors(allFinishing, selectedRooms, _selectedPhase);
        if(_errorWindowService.ShowNoticeWindow(mainErrors)) {
            return;
        }

        FinishingCalculator calculator = new FinishingCalculator(selectedRooms, allFinishing);

        ErrorsViewModel finishingErrors = _projectValidationService.CheckFinishingErrors(calculator, _selectedPhase);
        if(_errorWindowService.ShowNoticeWindow(finishingErrors)) {
            return;
        }

        List<FinishingElement> finishingElements = calculator.FinishingElements;
        using(Transaction t = _revitRepository.Document
            .StartTransaction(_localizationService.GetLocalizedString("MainWindow.TransactionName"))) {
            foreach(var element in finishingElements) {
                element.UpdateFinishingParameters();
                element.UpdateCategoryParameters();
            }
            t.Commit();
        }

        WarningsViewModel parameterErrors = _projectValidationService
            .CheckWarnings(selectedRooms, finishingElements, _selectedPhase);
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

    private void LoadConfig() {
        var settings = _pluginConfig.GetSettings(_revitRepository.Document);

        if(settings is null) {
            settings = _pluginConfig.AddSettings(_revitRepository.Document);
        }

        settings.Phase = SelectedPhase.Name;
        settings.RoomNames = RoomNames
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
        settings.RoomNames = RoomNames
            .Where(x => x.IsChecked)
            .Select(x => x.Name)
            .ToList();

        _pluginConfig.SaveProjectConfig();
    }
}
