using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitAreaBoundaries.Models;
using RevitAreaBoundaries.Models.Processors;
using RevitAreaBoundaries.Settings;

namespace RevitAreaBoundaries.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly IBoundaryProcessor _processor;
    
    private AreaBoundarySettings _areaBoundarySettings;
    
    private CommonSettingsViewModel _commonSettingsViewModel;
    private ViewPlanSelectionViewModel _viewPlanSelectionViewModel;
    private TypeElementSelectionViewModel _typeElementSelectionViewModel;

    private bool _hasViewErrors;
    private bool _hasElementErrors;
    private string _errorText;
    
    public MainViewModel(
        PluginConfig pluginConfig,
        SystemPluginConfig systemPluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        IBoundaryProcessor processor) {
        
        _pluginConfig = pluginConfig;
        _systemPluginConfig = systemPluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _processor = processor;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }
    
    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }
    
    public CommonSettingsViewModel CommonSettingsViewModel {
        get => _commonSettingsViewModel;
        set => RaiseAndSetIfChanged(ref _commonSettingsViewModel, value);
    }
    public ViewPlanSelectionViewModel ViewPlanSelectionViewModel {
        get => _viewPlanSelectionViewModel;
        set => RaiseAndSetIfChanged(ref _viewPlanSelectionViewModel, value);
    }
    public TypeElementSelectionViewModel TypeElementSelectionViewModel {
        get => _typeElementSelectionViewModel;
        set => RaiseAndSetIfChanged(ref _typeElementSelectionViewModel, value);
    }

    public bool HasViewErrors {
        get => _hasViewErrors;
        set => RaiseAndSetIfChanged(ref _hasViewErrors, value);
    }
    
    public bool HasElementErrors {
        get => _hasElementErrors;
        set => RaiseAndSetIfChanged(ref _hasElementErrors, value);
    }
    
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }
    
    private void LoadView() {
        LoadConfig();
        HasViewErrors = false;
        CommonSettingsViewModel = new CommonSettingsViewModel();
        ViewPlanSelectionViewModel = new ViewPlanSelectionViewModel(
            _localizationService,_systemPluginConfig, _revitRepository, _areaBoundarySettings);
        TypeElementSelectionViewModel = new TypeElementSelectionViewModel(
            _localizationService,_systemPluginConfig, _revitRepository, _areaBoundarySettings);
    }
    
    private void AcceptView() {
        SaveConfig();
        
        var view = _revitRepository.ActiveUiDocument.ActiveView;
        var boundarySettings = new AreaBoundarySettings { TargetViews = [view] };

        const string transactionName = "TransactionName";
        using var t = _revitRepository.Document.StartTransaction(transactionName);
        
        _processor.DrawBoundaries(boundarySettings);
        
        t.Commit();
    }
    
    private bool CanAcceptView() {
        if(ViewPlanSelectionViewModel != null) {
            if(ViewPlanSelectionViewModel.ViewPlanGroupViewModels.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.ErrorNoViewPlans");
                HasViewErrors = true;
                return false;
                
            }
            if(ViewPlanSelectionViewModel.SelectedViewPlanViewModels.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.ErrorNoSelectionViewPlans");
                HasViewErrors = true;
                return false;
            }
            HasViewErrors = false;
        }
        if(ViewPlanSelectionViewModel != null) {
            if(TypeElementSelectionViewModel.TypeElementGroupViewModels.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.ErrorNoElementTypes");
                HasElementErrors = true;
                return false;
                
            }
            if(TypeElementSelectionViewModel.SelectedTypeElementViewModels.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.ErrorNoSelectionElementTypes");
                HasElementErrors = true;
                return false;
            }
            HasElementErrors = false;
        }
        ErrorText = null;
        return true;
    }
    
    private void LoadConfig() {
        var projectConfig = _pluginConfig.GetSettings(_revitRepository.Document);
        ConfigSettings configSettings;
        if(projectConfig == null) {
            configSettings = new ConfigSettings();
            configSettings.ApplyDefaultValues(_systemPluginConfig);
        } else {
            configSettings = projectConfig.ConfigSettings;
        }

        _areaBoundarySettings = new AreaBoundarySettings {
            AlgorithmType = configSettings.AlgorithmType,
            SectionHeight = configSettings.SectionHeight,
            Views = configSettings.SelectedViewPlans,
            Types = configSettings.SelectedTypes,
            GroupParam = configSettings.GroupParam
        };
    }
    
    private void SaveConfig() {
        var configSettings = new ConfigSettings {
            AlgorithmType = _areaBoundarySettings.AlgorithmType,
            SectionHeight = _areaBoundarySettings.SectionHeight,
            SelectedViewPlans = _areaBoundarySettings.Views,
            SelectedTypes = _areaBoundarySettings.Types,
            GroupParam = _areaBoundarySettings.GroupParam
        };

        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                      ?? _pluginConfig.AddSettings(_revitRepository.Document);
        setting.ConfigSettings = configSettings;
        _pluginConfig.SaveProjectConfig();
    }
}
