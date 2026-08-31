using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitLintelsManager.Models;
using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class SettingsViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly ILocalizationService _localizationService;
    private readonly LintelManagerConfig _lintelManagerConfig;

    private string _errorText;
    private string _saveProperty;
    
    private bool _hasFamilySettingsErrors;
    private FamilySettingsViewModel _familySettingsViewModel;
   
    public SettingsViewModel(
        RevitRepository revitRepository,
        SystemPluginConfig systemPluginConfig,
        ILocalizationService localizationService,
        LintelManagerConfig lintelManagerConfig) {
        
        _revitRepository = revitRepository;
        _systemPluginConfig = systemPluginConfig;
        _localizationService = localizationService;
        _lintelManagerConfig = lintelManagerConfig;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }
   
    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }

    
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }
    
    public string SaveProperty {
        get => _saveProperty;
        set => RaiseAndSetIfChanged(ref _saveProperty, value);
    }
    
    public bool HasFamilySettingsErrors {
        get => _hasFamilySettingsErrors;
        set => RaiseAndSetIfChanged(ref _hasFamilySettingsErrors, value);
    }
    
    public FamilySettingsViewModel FamilySettingsViewModel {
        get => _familySettingsViewModel;
        set => RaiseAndSetIfChanged(ref _familySettingsViewModel, value);
    }
   
    private void LoadView() {
        LoadConfig();
        
        FamilySettingsViewModel = new FamilySettingsViewModel(_revitRepository, _systemPluginConfig, _localizationService);
    }
    
    private void AcceptView() {
        SaveConfig();
        
        int fff = FamilySettingsViewModel.SelectedOpeningFamilyViewModels.Count;
        
        System.Windows.MessageBox.Show($"Selected: {fff}");
    }

    
    private bool CanAcceptView() {
        if(string.IsNullOrEmpty(SaveProperty)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.HelloCheck");
            return false;
        }

        ErrorText = null;
        return true;
    }

    
    private void LoadConfig() {
    }

    
    private void SaveConfig() {
    }
}
