using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRoundingOfAreas.Models;
using RevitRoundingOfAreas.Models.Enums;
using RevitRoundingOfAreas.Models.Factories;
using RevitRoundingOfAreas.Models.Interfaces;

namespace RevitRoundingOfAreas.ViewModels;

internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ParamService _paramService;
    private readonly ProvidersFactory _providersFactory;
    private readonly SpatialElementCheckService _spatialElementCheckService;
    private readonly IWindowService _windowService;
    private readonly ILocalizationService _localizationService;
    private ConfigSettings _configSettings;
    private ObservableCollection<RangeViewModel> _range;
    private RangeViewModel _selectedRange;
    private ObservableCollection<PhaseViewModel> _phases;
    private PhaseViewModel _selectedPhase;
    private ObservableCollection<ParamViewModel> _sourceParams;
    private ParamViewModel _selectedSourceParam;
    private ObservableCollection<ParamViewModel> _targetParams;
    private ParamViewModel _selectedTargetParam;
    private ObservableCollection<AccuracyViewModel> _accuracy;
    private AccuracyViewModel _selectedAccuracy;
    private string _errorText;

    public MainViewModel(
        PluginConfig pluginConfig,
        SystemPluginConfig systemPluginConfig,
        RevitRepository revitRepository,
        ParamService paramService,
        ProvidersFactory providersFactory,
        ILocalizationService localizationService,
        IWindowService windowService,
        SpatialElementCheckService spatialElementCheckService) {

        _pluginConfig = pluginConfig;
        _systemPluginConfig = systemPluginConfig;
        _revitRepository = revitRepository;
        _paramService = paramService;
        _providersFactory = providersFactory;
        _localizationService = localizationService;
        _windowService = windowService;
        _spatialElementCheckService = spatialElementCheckService;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }


    public ObservableCollection<RangeViewModel> Range {
        get => _range;
        set => RaiseAndSetIfChanged(ref _range, value);
    }
    public RangeViewModel SelectedRange {
        get => _selectedRange;
        set => RaiseAndSetIfChanged(ref _selectedRange, value);
    }
    public ObservableCollection<PhaseViewModel> Phases {
        get => _phases;
        set => RaiseAndSetIfChanged(ref _phases, value);
    }
    public PhaseViewModel SelectedPhase {
        get => _selectedPhase;
        set => RaiseAndSetIfChanged(ref _selectedPhase, value);
    }
    public ObservableCollection<ParamViewModel> SourceParams {
        get => _sourceParams;
        set => RaiseAndSetIfChanged(ref _sourceParams, value);
    }
    public ParamViewModel SelectedSourceParam {
        get => _selectedSourceParam;
        set => RaiseAndSetIfChanged(ref _selectedSourceParam, value);
    }
    public ObservableCollection<ParamViewModel> TargetParams {
        get => _targetParams;
        set => RaiseAndSetIfChanged(ref _targetParams, value);
    }
    public ParamViewModel SelectedTargetParam {
        get => _selectedTargetParam;
        set => RaiseAndSetIfChanged(ref _selectedTargetParam, value);
    }
    public ObservableCollection<AccuracyViewModel> Accuracy {
        get => _accuracy;
        set => RaiseAndSetIfChanged(ref _accuracy, value);
    }
    public AccuracyViewModel SelectedAccuracy {
        get => _selectedAccuracy;
        set => RaiseAndSetIfChanged(ref _selectedAccuracy, value);
    }
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    // Метод получения коллекции AccuracyViewModel для Accuracy
    private IEnumerable<AccuracyViewModel> GetAccuracyViewModels() {
        for(int i = 1; i <= _systemPluginConfig.DefaultAccuracyRange; i++) {
            yield return new AccuracyViewModel {
                Accuracy = i,
                Name = i.ToString(),
            };
        }
    }

    // Метод получения коллекции ParamViewModel для SourceParams
    private IEnumerable<ParamViewModel> GetSourceParamViewModels() {
        return _paramService.AllRevitParams
            .Select(param => new ParamViewModel {
                Name = param.Name,
                RevitParam = param
            });
    }

    // Метод получения коллекции ParamViewModel для TargetParams
    private IEnumerable<ParamViewModel> GetTargetParamViewModels() {
        return _paramService.AllRevitParams
            .Where(revitParam => {
                var spatialElements = _revitRepository.GetAllSpatialElements();
                var randomSpatialElement = spatialElements.FirstOrDefault();
                if(randomSpatialElement is null) {
                    return false;
                }
                var param = revitParam.GetParam(randomSpatialElement);
                return !param.IsReadOnly;
            })
            .Select(param => new ParamViewModel {
                Name = param.Name,
                RevitParam = param
            });
    }

    // Метод получения коллекции PhaseViewModel для Phases
    private IEnumerable<PhaseViewModel> GetPhaseViewModels() {
        return _revitRepository.GetPhaseModels()
            .Select(phase => new PhaseViewModel {
                Name = phase.Name,
                ElementId = phase.ElementId
            });
    }

    // Метод получения коллекции RangeElementsViewModel для RangeElements
    private IEnumerable<RangeViewModel> GetRangeViewModels() {
        var providers = Enum.GetValues(typeof(ElementsProviderType)).Cast<ElementsProviderType>();
        return providers
            .Select(provider => new RangeViewModel {
                Name = _localizationService.GetLocalizedString($"MainViewModel.{provider}"),
                ElementsProvider = _providersFactory.GetElementsProvider(provider)
            });
    }

    private void LoadView() {
        LoadConfig();

        Range = new ObservableCollection<RangeViewModel>(GetRangeViewModels());
        SelectedRange = ResolveDefaultRange() ?? Range.FirstOrDefault();

        Phases = new ObservableCollection<PhaseViewModel>(GetPhaseViewModels());
        SelectedPhase = Phases
            .FirstOrDefault(phase => phase.ElementId == _configSettings.SelectedPhaseId)
            ?? Phases.LastOrDefault();

        SourceParams = new ObservableCollection<ParamViewModel>(GetSourceParamViewModels());
        SelectedSourceParam = SourceParams
            .FirstOrDefault(param => param.RevitParam.Id == _configSettings.SourceParam?.Id)
            ?? SourceParams.FirstOrDefault();

        TargetParams = new ObservableCollection<ParamViewModel>(GetTargetParamViewModels());
        SelectedTargetParam = TargetParams
            .FirstOrDefault(param => param.RevitParam.Id == _configSettings.TargetParam?.Id)
            ?? TargetParams.FirstOrDefault();

        Accuracy = new ObservableCollection<AccuracyViewModel>(GetAccuracyViewModels());
        SelectedAccuracy = Accuracy
            .FirstOrDefault(accuracy => accuracy.Accuracy == _configSettings.Accuracy)
            ?? Accuracy.FirstOrDefault();
    }

    private RangeViewModel ResolveDefaultRange() {
        return _revitRepository.HasSelectedRooms()
            ? Range.FirstOrDefault(x => x.ElementsProvider.Type == ElementsProviderType.SelectedElementsProvider)
            : _revitRepository.HasRoomsOnCurrentView()
            ? Range.FirstOrDefault(x => x.ElementsProvider.Type == ElementsProviderType.CurrentViewProvider)
            : Range.FirstOrDefault(x => x.ElementsProvider.Type == ElementsProviderType.AllElementsProvider);
    }

    private void AcceptView() {
        SaveConfig();
        var spatialElements = SelectedRange.ElementsProvider.GetSpatialElements(SelectedPhase.ElementId);
        var warnings = _spatialElementCheckService.CheckSpatialElements(spatialElements);

        if(warnings.Count != 0) {
            _windowService.CloseMainWindow();
            _windowService.ShowWarningWindow(warnings);
            return;
        }

        string transactionName = _localizationService.GetLocalizedString("MainViewModel.TransactionName");
        using var t = _revitRepository.Document.StartTransaction(transactionName);
        _revitRepository.RoundingOfAreas(
            spatialElements,
            SelectedSourceParam.RevitParam,
            SelectedTargetParam.RevitParam,
            SelectedAccuracy.Accuracy
        );
        t.Commit();
    }

    private bool CanAcceptView() {
        if(SelectedRange != null) {
            if(SelectedRange.ElementsProvider.Type == ElementsProviderType.SelectedElementsProvider
                && !_revitRepository.HasSelectedRooms()) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoSelection");
                return false;
            }
            if(SelectedRange.ElementsProvider.Type == ElementsProviderType.CurrentViewProvider
                && !_revitRepository.HasRoomsOnCurrentView()) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoRoomsOnView");
                return false;
            }
        }
        ErrorText = null;
        return true;
    }

    private void LoadConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document);
        _configSettings = setting?.ConfigSettings ?? new ConfigSettings();
        ApplyDefaultsConfig();
    }

    // Метод применения дефолтных значений настроек.
    private void ApplyDefaultsConfig() {
        if(_configSettings.SelectedPhaseId is null || _configSettings.SelectedPhaseId == ElementId.InvalidElementId) {
            _configSettings.SelectedPhaseId = _revitRepository.GetPhaseIdByName(_systemPluginConfig.DefaultPhaseName);
        }

        _configSettings.SourceParam ??= _paramService.DefaultSourceParam;
        _configSettings.TargetParam ??= _paramService.DefaultTargetParam;

        if(_configSettings.Accuracy <= 0) {
            _configSettings.Accuracy = _systemPluginConfig.DefaultAccuracy;
        }
    }

    private void SaveConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                      ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.ConfigSettings = new ConfigSettings {
            SelectedPhaseId = SelectedPhase?.ElementId ?? ElementId.InvalidElementId,
            SourceParam = SelectedSourceParam?.RevitParam,
            TargetParam = SelectedTargetParam?.RevitParam,
            Accuracy = SelectedAccuracy?.Accuracy ?? _systemPluginConfig.DefaultAccuracy
        };

        _pluginConfig.SaveProjectConfig();
    }
}
