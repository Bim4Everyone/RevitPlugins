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

    private bool _hasErrors;
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

    public bool HasErrors {
        get => _hasErrors;
        set => RaiseAndSetIfChanged(ref _hasErrors, value);
    }
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }
    
    private void LoadView() {
        LoadConfig();
        CommonSettingsViewModel = new CommonSettingsViewModel();
        CommonSettingsViewModel.PropertyChanged += CommonSettingsViewModelChanged;
        ViewPlanSelectionViewModel = new ViewPlanSelectionViewModel(_revitRepository, _areaBoundarySettings);
        ViewPlanSelectionViewModel.PropertyChanged += ViewPlanSelectionViewModelChanged;
        TypeElementSelectionViewModel = new TypeElementSelectionViewModel();
        TypeElementSelectionViewModel.PropertyChanged += TypeElementSelectionViewModelChanged;
    }
    
    private void CommonSettingsViewModelChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
        
    }

    private void ViewPlanSelectionViewModelChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
        
    }
    
    private void TypeElementSelectionViewModelChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
        
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
            Types = configSettings.SelectedTypes
                
        };
    }
    
    private void SaveConfig() {
        var configSettings = new ConfigSettings {
            AlgorithmType = _areaBoundarySettings.AlgorithmType,
            SectionHeight = _areaBoundarySettings.SectionHeight,
            SelectedViewPlans = _areaBoundarySettings.Views,
            SelectedTypes = _areaBoundarySettings.Types
        };

        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                      ?? _pluginConfig.AddSettings(_revitRepository.Document);
        setting.ConfigSettings = configSettings;
        _pluginConfig.SaveProjectConfig();
    }
}
