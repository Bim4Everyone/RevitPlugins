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
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string _errorText;
    private string _saveProperty;
    
    private bool _hasFamilySettingsErrors;
    private FamilySettingsViewModel _familySettingsViewModel;
   
    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService) {
        
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;

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
        
        FamilySettingsViewModel = new FamilySettingsViewModel(_revitRepository, _localizationService);
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
