using System;
using System.Linq;
using System.Windows.Input;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitAreaBoundaries.Models;
using RevitAreaBoundaries.Models.Processors;
using RevitAreaBoundaries.Services;
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
    private readonly BoundaryProcessorSelector _boundaryProcessorSelector;
    
    private ConfigSettings _configSettings;
    private AreaBoundarySettings _areaBoundarySettings;
    
    private CommonSettingsViewModel _commonSettingsViewModel;
    private ViewPlanSelectionViewModel _viewPlanSelectionViewModel;
    private TypeElementSelectionViewModel _typeElementSelectionViewModel;

    private bool _hasSettingsErrors;
    private bool _hasViewErrors;
    private bool _hasElementErrors;
    private string _errorText;
    
    public MainViewModel(
        PluginConfig pluginConfig,
        SystemPluginConfig systemPluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        IProgressDialogFactory progressDialogFactory,
        BoundaryProcessorSelector boundaryProcessorSelector) {
        
        _pluginConfig = pluginConfig;
        _systemPluginConfig = systemPluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _boundaryProcessorSelector = boundaryProcessorSelector;
        
        ProgressDialogFactory = progressDialogFactory
                                ?? throw new ArgumentNullException(nameof(progressDialogFactory));

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }
    
    public IProgressDialogFactory ProgressDialogFactory { get; }
    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }
    
    public CommonSettingsViewModel CommonSettingsViewModel {
        get => _commonSettingsViewModel;
        private set => RaiseAndSetIfChanged(ref _commonSettingsViewModel, value);
    }
    public ViewPlanSelectionViewModel ViewPlanSelectionViewModel {
        get => _viewPlanSelectionViewModel;
        private set => RaiseAndSetIfChanged(ref _viewPlanSelectionViewModel, value);
    }
    public TypeElementSelectionViewModel TypeElementSelectionViewModel {
        get => _typeElementSelectionViewModel;
        private set => RaiseAndSetIfChanged(ref _typeElementSelectionViewModel, value);
    }
    
    public bool HasSettingsErrors {
        get => _hasSettingsErrors;
        set => RaiseAndSetIfChanged(ref _hasSettingsErrors, value);
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
        private set => RaiseAndSetIfChanged(ref _errorText, value);
    }
    
    private void LoadView() {
        LoadConfig();
        HasViewErrors = false;
        CommonSettingsViewModel = new CommonSettingsViewModel(_localizationService, _configSettings);
        ViewPlanSelectionViewModel = new ViewPlanSelectionViewModel(
            _localizationService,_systemPluginConfig, _revitRepository, _configSettings);
        TypeElementSelectionViewModel = new TypeElementSelectionViewModel(
            _revitRepository, _configSettings);
    }
    
    private void AcceptView() {
        SaveSettings();
        SaveConfig();
        
        using var progressDialogService = ProgressDialogFactory.CreateDialog();
        var progress = progressDialogService.CreateProgress();
        var ct = progressDialogService.CreateCancellationToken();
        //
        // var progressService = new ProgressService(_localizationService) {
        //     CancellationToken = ct,
        //     ProgressCount = progress,
        //     SetupStage = (text, max, step) => {
        //         progressDialogService.DisplayTitleFormat = text;
        //         progressDialogService.MaxValue = max;
        //         progressDialogService.StepValue = step;
        //     }
        // };
        
        progressDialogService.Show();
        
        var processor = _boundaryProcessorSelector.SelectProcessor(_areaBoundarySettings);
        
        string transactionName = _localizationService.GetLocalizedString("MainViewModel.TransactionName");
        using var t = _revitRepository.Document.StartTransaction(transactionName);
        
        processor.DrawBoundaries(_areaBoundarySettings);
        
        t.Commit();
    }
    
    private bool CanAcceptView() {
        if(CommonSettingsViewModel != null) {
            if(!double.TryParse(CommonSettingsViewModel.SectionHeight, out double sectionHeight)) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.SectionHeightNoDouble");
                HasSettingsErrors = true;
                return false;
            }
            if(sectionHeight <= 0) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.SectionHeightNegate");
                HasSettingsErrors = true;
                return false;
            }
            HasSettingsErrors = false;
        }
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
        if(TypeElementSelectionViewModel != null) {
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
        if(projectConfig == null) {
            _configSettings = new ConfigSettings();
            _configSettings.ApplyDefaultValues(_systemPluginConfig);
        } else {
            _configSettings = projectConfig.ConfigSettings;
        }
    }
    
    // Метод сохранения настроек
    private void SaveSettings() {
        var algorithmType = CommonSettingsViewModel.SelectedAlgorithmTypeViewModel.AlgorithmType;
        double sectionHeight = double.Parse(CommonSettingsViewModel.SectionHeight);
        var views = ViewPlanSelectionViewModel.SelectedViewPlanViewModels
            .Select(vm => vm.RevitElement).ToList();
        var types = TypeElementSelectionViewModel.SelectedTypeElementViewModels
            .Select(vm => vm.RevitElement).ToList();
        string groupParam = ViewPlanSelectionViewModel.SelectedGroupParamViewModel.Name;

        _areaBoundarySettings = new AreaBoundarySettings {
            AlgorithmType = algorithmType,
            SectionHeight = sectionHeight,
            Views = views,
            Types = types,
            GroupParam = groupParam
        };
    }
    
    private void SaveConfig() {
        var configSettings = new ConfigSettings {
            AlgorithmType = _areaBoundarySettings.AlgorithmType,
            SectionHeight = _areaBoundarySettings.SectionHeight,
            Views = _areaBoundarySettings.Views.Select(view => view.Element.Id).ToList(),
            Types = _areaBoundarySettings.Types.Select(view => view.Element.Id).ToList(),
            GroupParam = _areaBoundarySettings.GroupParam
        };

        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                      ?? _pluginConfig.AddSettings(_revitRepository.Document);
        setting.ConfigSettings = configSettings;
        _pluginConfig.SaveProjectConfig();
    }
}
