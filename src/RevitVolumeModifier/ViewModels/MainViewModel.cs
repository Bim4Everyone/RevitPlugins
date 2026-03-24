using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitVolumeModifier.Enums;
using RevitVolumeModifier.Interfaces;
using RevitVolumeModifier.Models;
using RevitVolumeModifier.Services;

namespace RevitVolumeModifier.ViewModels;

internal class MainViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly IParamAvailabilityService _paramAvailabilityService;
    private readonly IRevitParamFactory _revitParamFactory;
    private readonly RevitRepository _revitRepository;
    private readonly RevitPickService _revitPickService;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly PluginConfig _pluginConfig;

    private Document _initialDocument;
    private bool _initialDocumentChanged;
    private List<ParamModel> _paramModels;
    private ICollection<ElementId> _elementIds;
    private ObservableCollection<ParamViewModel> _paramViewModels;
    private bool _hasParamWarning;
    private string _errorText;
    private CommandStateViewModel _commandState;

    public MainViewModel(
        ILocalizationService localizationService,
        IParamAvailabilityService paramAvailabilityService,
        IRevitParamFactory revitParamFactory,
        RevitRepository revitRepository,
        RevitPickService revitPickService,
        SystemPluginConfig systemPluginConfig,
        PluginConfig pluginConfig) {

        _localizationService = localizationService;
        _paramAvailabilityService = paramAvailabilityService;
        _revitParamFactory = revitParamFactory;
        _revitRepository = revitRepository;
        _revitPickService = revitPickService;
        _systemPluginConfig = systemPluginConfig;
        _pluginConfig = pluginConfig;

        LoadViewCommand = RelayCommand.Create(LoadView);
        SaveConfigCommand = RelayCommand.Create(SaveConfig);
        JoinCommand = RelayCommand.Create(Join, CanExecute);
        DivideBySelectHorizontalPointCommand = RelayCommand.Create(DivideBySelectHorizontalPoint, CanExecute);
        DivideBySelectVerticalPointCommand = RelayCommand.Create(DivideBySelectVerticalPoint, CanExecute);
        DivideBySelectFacesCommand = RelayCommand.Create(DivideBySelectFacesPoint, CanExecute);
        CutCommand = RelayCommand.Create(Cut, CanExecute);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand SaveConfigCommand { get; }
    public ICommand JoinCommand { get; }
    public ICommand DivideBySelectHorizontalPointCommand { get; }
    public ICommand DivideBySelectVerticalPointCommand { get; }
    public ICommand DivideBySelectFacesCommand { get; }
    public ICommand CutCommand { get; }

    public bool InitialDocumentChanged {
        get => _initialDocumentChanged;
        set => RaiseAndSetIfChanged(ref _initialDocumentChanged, value);
    }
    public ICollection<ElementId> ElementIds {
        get => _elementIds;
        set => RaiseAndSetIfChanged(ref _elementIds, value);
    }
    public ObservableCollection<ParamViewModel> ParamViewModels {
        get => _paramViewModels;
        set => RaiseAndSetIfChanged(ref _paramViewModels, value);
    }
    public bool HasParamWarning {
        get => _hasParamWarning;
        set => RaiseAndSetIfChanged(ref _hasParamWarning, value);
    }
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }
    public CommandStateViewModel CommandState {
        get => _commandState;
        set => RaiseAndSetIfChanged(ref _commandState, value);
    }

    // Метод обновления предупреждений в параметрах
    private void UpdateParamWarnings() {
        foreach(var paramViewModel in ParamViewModels) {
            paramViewModel.UpdateWarning(_revitRepository.Document, _localizationService, _paramAvailabilityService);
        }
        HasParamWarning = ParamViewModels.Any(param => param.HasWarning);
    }

    // Метод подписанный на событие изменения ParamViewModel
    private void OnParamChanged(object sender, PropertyChangedEventArgs e) {
        if(sender is not ParamViewModel vm) {
            return;
        }
        switch(e.PropertyName) {
            case nameof(ParamViewModel.RevitParamName):
                UpdateParamWarnings();
                if(!vm.HasWarning) {
                    var def = _paramAvailabilityService.GetDefinitionByName(_revitRepository.Document, vm.RevitParamName);
                    var newParam = _revitParamFactory.Create(_revitRepository.Document, def.GetElementId());
                    vm.ParamModel.RevitParam = newParam;
                }
                break;
        }
    }

    // Метод получения коллекции ParamViewModel для Params
    private IEnumerable<ParamViewModel> GetParamViewModels() {
        return _paramModels
            .Select(param => new ParamViewModel {
                ParamModel = param,
                RevitParamName = param.RevitParam.Name,
                Description = _localizationService.GetLocalizedString($"MainViewModel.{param.ParamType}Description"),
                DetailDescription = _localizationService.GetLocalizedString($"MainViewModel.{param.ParamType}DetailDescription")
            });
    }

    // Метод объединения объемов
    private async void Join() {
        ResetCommandState();
        var commandType = CommandType.Join;
        var parameters = ParamViewModels.Select(p => p.ParamModel);
        bool result = await _revitRepository.Join(ElementIds, parameters);
        UpdateCommandState(result, commandType);
    }

    // Метод разделения объемов по горизонтальной точке
    private async void DivideBySelectHorizontalPoint() {
        ResetCommandState();
        var commandType = CommandType.DivideByHorPoint;
        var parameters = ParamViewModels.Select(p => p.ParamModel);
        string promt = _localizationService.GetLocalizedString("MainViewModel.PickPointOnElementPromt");
        var reference = await _revitPickService.PickPointOnElementAsync(promt);
        if(reference == null) {
            UpdateCommandStateFailed(commandType);
            return;
        }
        bool result = await _revitRepository.DivideByHorizontalPointAsync(ElementIds, reference, parameters);
        UpdateCommandState(result, commandType);
    }

    // Метод разделения объемов по вертикальной точке
    private async void DivideBySelectVerticalPoint() {
        ResetCommandState();
        var commandType = CommandType.DivideByVertPoint;
        var parameters = ParamViewModels.Select(p => p.ParamModel);
        string promt = _localizationService.GetLocalizedString("MainViewModel.PickPointOnElementPromt");
        var reference = await _revitPickService.PickPointOnElementAsync(promt);
        if(reference == null) {
            UpdateCommandStateFailed(commandType);
            return;
        }
        bool result = await _revitRepository.DivideByVerticalPointAsync(ElementIds, reference, parameters);
        UpdateCommandState(result, commandType);
    }

    // Метод разделения объемов по граням
    private async void DivideBySelectFacesPoint() {
        ResetCommandState();
        var commandType = CommandType.DivideByFaces;
        var parameters = ParamViewModels.Select(p => p.ParamModel);
        string promt = _localizationService.GetLocalizedString("MainViewModel.PickFacesPromt");
        var fases = await _revitPickService.PickFacesMultipleOnElementAsync(promt);
        if(fases == null) {
            UpdateCommandStateFailed(commandType);
            return;
        }
        bool result = await _revitRepository.DivideByFacesAsync(ElementIds, fases, parameters);
        UpdateCommandState(result, commandType);
    }

    // Метод вырезания объемов
    private async void Cut() {
        ResetCommandState();
        var commandType = CommandType.Cut;
        var parameters = ParamViewModels.Select(p => p.ParamModel);
        string promt = _localizationService.GetLocalizedString("MainViewModel.CutPromt");
        var elements = await _revitPickService.PickGenericModelsAsync(promt);
        if(elements == null) {
            UpdateCommandStateFailed(commandType);
            return;
        }
        bool result = await _revitRepository.CutAsync(ElementIds, elements, parameters);
        UpdateCommandState(result, commandType);
    }

    // Метод проверки выполнения всех методов
    private bool CanExecute() {
        if(InitialDocumentChanged) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.DocumentChangedWarning");
            return false;
        }
        if(HasParamWarning) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.HasParamWarning");
            return false;
        }
        if(ElementIds == null || ElementIds.Count == 0) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoSelection");
            return false;
        }
        ErrorText = null;
        return true;
    }

    // Метод подписанный на события изменения выделения
    public void OnSelectionChanged(ICollection<ElementId> selection) {
        ElementIds = selection;
        CommandManager.InvalidateRequerySuggested();
    }

    // Метод подписанный на события смены документа
    public void OnDocumentChanged(Document doc) {
        InitialDocumentChanged = doc.GetUniqId() != _initialDocument.GetUniqId();
        UpdateParamWarnings();
        ElementIds = null;
        CommandManager.InvalidateRequerySuggested();
    }

    // Метод загрузки вида
    public void LoadView() {
        LoadConfig();
        ParamViewModels = new ObservableCollection<ParamViewModel>(GetParamViewModels());
        foreach(var param in ParamViewModels) {
            param.PropertyChanged += OnParamChanged;
        }
        UpdateParamWarnings();

        CommandState = new CommandStateViewModel();
        ResetCommandState();
        _initialDocument = _revitRepository.Document;
        InitialDocumentChanged = false;
    }

    // Метод обновления CommandState в зависимости от результата
    private void UpdateCommandState(bool result, CommandType commandType) {
        if(result) {
            UpdateCommandStateSuccess(commandType);
        } else {
            UpdateCommandStateFailed(commandType);
        }
    }

    // Метод сброса CommandState
    private void ResetCommandState() {
        CommandState.CommandStatus = CommandStatus.None;
        CommandState.CommandType = CommandType.None;
        CommandState.CommandText = string.Empty;
    }

    // Метод обновления CommandState при положительном результате
    private void UpdateCommandStateSuccess(CommandType commandType) {
        CommandState.CommandStatus = CommandStatus.Success;
        CommandState.CommandType = commandType;
        CommandState.CommandText = _localizationService.GetLocalizedString("MainViewModel.SuccessCommand");
    }

    // Метод обновления CommandState при отрицательном результате
    private void UpdateCommandStateFailed(CommandType commandType) {
        CommandState.CommandStatus = CommandStatus.Failed;
        CommandState.CommandType = commandType;
        CommandState.CommandText = _localizationService.GetLocalizedString("MainViewModel.FailedCommand");
    }

    // Метод загрузки конфигурации
    private void LoadConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document);
        _paramModels = setting?.ParamModels ?? _systemPluginConfig.GetDefaultParams();
        System.Windows.MessageBox.Show(_paramModels.Count.ToString());
    }

    // Метод сохранения конфигурации
    public void SaveConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);
        setting.ParamModels = ParamViewModels.Select(vm => vm.ParamModel).ToList();
        _pluginConfig.SaveProjectConfig();
    }
}
