using System.Linq;
using System.Windows.Input;

using dosymep.Revit.Comparators;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitDeclarations.Models;
using RevitDeclarations.Services;

namespace RevitDeclarations.ViewModels;
internal abstract class MainViewModel : BaseViewModel {
    protected readonly RevitRepository _revitRepository;
    protected readonly DeclarationSettings _settings;
    protected readonly ILocalizationService _localizationService;
    protected readonly IMessageBoxService _messageBoxService;
    protected readonly ErrorWindowService _errorWindowService;

    protected ParametersViewModel _parametersViewModel;
    protected PrioritiesViewModel _prioritiesViewModel;
    protected DeclarationViewModel _declarationViewModel;

    protected LogicalStringComparer _stringComparer;

    private bool _hasDeclarationPageErrors;
    private bool _hasParametersPageErrors;
    private string _errorText;

    public MainViewModel(RevitRepository revitRepository, 
                         DeclarationSettings settings,
                         ILocalizationService localizationService,
                         IMessageBoxService messageBoxService,
                         ErrorWindowService errorWindowService) {
        _revitRepository = revitRepository;
        _settings = settings;
        _localizationService = localizationService;
        _messageBoxService = messageBoxService;
        _errorWindowService = errorWindowService;

        _stringComparer = new LogicalStringComparer();

        ExportDeclarationCommand = new RelayCommand(ExportDeclaration, CanExport);
    }

    public ICommand ExportDeclarationCommand { get; }

    public DeclarationViewModel DeclarationViewModel => _declarationViewModel;
    public ParametersViewModel ParametersViewModel => _parametersViewModel;
    public PrioritiesViewModel PrioritiesViewModel => _prioritiesViewModel;

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public bool HasDeclarationPageErrors {
        get => _hasDeclarationPageErrors;
        set {
            if(ValidateFilePath()) {
                value = true;
            }
            if(ValidateFileName()) {
                value = true;
            }
            if(ValidateCheckedDocuments()) {
                value = true;
            }
            if(ValidatePhases()) {
                value = true;
            }
            RaiseAndSetIfChanged(ref _hasDeclarationPageErrors, value);
        }
    }

    public bool HasParametersPageErrors {
        get => _hasParametersPageErrors;
        set {
            if(ValidateEmptyParameters()) {
                value = true;
            }
            if(ValidateFilterValues()) {
                value = true;
            }
            if(ValidateProjectName()) {
                value = true;
            }
            RaiseAndSetIfChanged(ref _hasParametersPageErrors, value);
        }
    }

    public abstract void ExportDeclaration(object obj);

    public bool CanExport(object obj) {
        HasDeclarationPageErrors = false;
        HasParametersPageErrors = false;

        if(ValidateFilePath()) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoFolder");
            return false;
        }
        if(ValidateFileName()) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoName");
            return false;
        }
        if(ValidateCheckedDocuments()) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoProjects");
            return false;
        }
        if(ValidatePhases()) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoPhaseInDocs");
            return false;
        }
        if(ValidateEmptyParameters()) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoParams");
            return false;
        }
        if(ValidateFilterValues()) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoParamFilters");
            return false;
        }
        if(ValidateProjectName()) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoProjectId");
            return false;
        }

        ErrorText = "";
        return true;
    }

    public void SetSelectedSettings() {
        int.TryParse(_declarationViewModel.Accuracy, out int accuracy);
        _settings.AccuracyForArea = accuracy;
        _settings.AccuracyForLength = 2;
        _settings.SelectedPhase = _declarationViewModel.SelectedPhase;

        _settings.PrioritiesConfig = _prioritiesViewModel.PrioritiesConfig;

        _settings.LoadUtp = _declarationViewModel.LoadUtp;

        _settings.FilterRoomsParam = _parametersViewModel.SelectedFilterRoomsParam;
        _settings.FilterRoomsValues = _parametersViewModel.FilterRoomsValues.Select(x => x.Value).ToArray();
        _settings.GroupingBySectionParam = _parametersViewModel.SelectedGroupingBySectionParam;
        _settings.GroupingByGroupParam = _parametersViewModel.SelectedGroupingByGroupParam;
        _settings.MultiStoreyParam = _parametersViewModel.SelectedMultiStoreyParam;
        _settings.DepartmentParam = _parametersViewModel.SelectedDepartmentParam;
        _settings.LevelParam = _parametersViewModel.SelectedLevelParam;
        _settings.ApartmentNumberParam = _parametersViewModel.SelectedApartNumParam;
        _settings.SectionParam = _parametersViewModel.SelectedSectionParam;
        _settings.BuildingParam = _parametersViewModel.SelectedBuildingParam;
        _settings.ApartmentAreaParam = _parametersViewModel.SelectedApartAreaParam;
        _settings.ProjectName = _parametersViewModel.ProjectName;
        _settings.RoomAreaParam = _parametersViewModel.SelectedRoomAreaParam;
        _settings.RoomAreaCoefParam = _parametersViewModel.SelectedRoomAreaParam;
        _settings.RoomNameParam = _parametersViewModel.SelectedRoomNameParam;
        _settings.RoomNumberParam = _parametersViewModel.SelectedRoomNumberParam;

        _settings.AllParameters = _parametersViewModel.AllSelectedParameters;
    }

    /// <summary>
    /// Сохранение настроек вкладки DeclarationTabItem.
    /// Эти настройки одинаковы для всех деклараций.
    /// </summary>
    public void SaveMainWindowConfig(DeclarationConfigSettings configSettings) {
        configSettings.DeclarationName = _declarationViewModel.FileName;
        configSettings.DeclarationPath = _declarationViewModel.FilePath;
        configSettings.ExportFormat = _declarationViewModel.SelectedFormat.Id;
        configSettings.Phase = _declarationViewModel.SelectedPhase.Name;

        configSettings.RevitDocuments = _declarationViewModel.RevitDocuments
            .Where(x => x.IsChecked)
            .Select(x => x.Name)
            .ToList();
    }

    /// <summary>
    /// Сохранение настроек вкладки ParamsTabItem.
    /// Сохраняются настройки, общие для всех деклараций.
    /// </summary>
    public void SaveParametersConfig(DeclarationConfigSettings configSettings) {
        configSettings.FilterRoomsParam = _settings.FilterRoomsParam?.Definition.Name;
        configSettings.FilterRoomsValues = _settings.FilterRoomsValues;
        configSettings.GroupingBySectionParam = _settings.GroupingBySectionParam?.Definition.Name;
        configSettings.GroupingByGroupParam = _settings.GroupingByGroupParam?.Definition.Name;
        configSettings.MultiStoreyParam = _settings.MultiStoreyParam?.Definition.Name;

        configSettings.DepartmentParam = _settings.DepartmentParam?.Definition.Name;
        configSettings.LevelParam = _settings.LevelParam?.Definition.Name;
        configSettings.SectionParam = _settings.SectionParam?.Definition.Name;
        configSettings.BuildingParam = _settings.BuildingParam?.Definition.Name;

        configSettings.ApartmentNumberParam = _settings.ApartmentNumberParam?.Definition.Name;
        configSettings.ApartmentAreaParam = _settings.ApartmentAreaParam?.Definition.Name;

        configSettings.RoomAreaParam = _settings.RoomAreaParam?.Definition.Name;
        configSettings.RoomNameParam = _settings.RoomNameParam?.Definition.Name;

        configSettings.ProjectNameID = _settings.ProjectName;
    }

    public void LoadMainWindowConfig(DeclarationConfigSettings configSettings) {
        _declarationViewModel.FileName = configSettings.DeclarationName;
        _declarationViewModel.FilePath = configSettings.DeclarationPath;
        _declarationViewModel.SelectedFormat = _declarationViewModel.ExportFormats
            .FirstOrDefault(x => x.Id == configSettings.ExportFormat) ?? _declarationViewModel.ExportFormats.FirstOrDefault();
        _declarationViewModel.SelectedPhase = _declarationViewModel.Phases
            .FirstOrDefault(x => x.Name == configSettings.Phase) ?? _declarationViewModel
            .Phases[_declarationViewModel.Phases.Count - 1];

        var documents = _declarationViewModel.RevitDocuments
            .Where(x => configSettings.RevitDocuments.Contains(x.Name));

        foreach(var document in documents) {
            document.IsChecked = true;
        }
    }

    private bool ValidateFilePath() {
        return string.IsNullOrEmpty(_declarationViewModel.FilePath);
    }

    private bool ValidateFileName() {
        return string.IsNullOrEmpty(_declarationViewModel.FileName);
    }

    private bool ValidatePhases() {
        return !_declarationViewModel.RevitDocuments
            .Where(x => x.IsChecked)
            .All(x => x.HasPhase(_declarationViewModel.SelectedPhase));
    }

    private bool ValidateCheckedDocuments() {
        return !_declarationViewModel.RevitDocuments
            .Where(x => x.IsChecked)
            .Any();
    }

    private bool ValidateEmptyParameters() {
        return _parametersViewModel
            .AllSelectedParameters
            .Where(x => x == null)
            .Any();
    }

    private bool ValidateFilterValues() {
        return !_parametersViewModel.FilterRoomsValues.Any();
    }

    private bool ValidateProjectName() {
        return string.IsNullOrEmpty(_parametersViewModel.ProjectName);
    }
}
