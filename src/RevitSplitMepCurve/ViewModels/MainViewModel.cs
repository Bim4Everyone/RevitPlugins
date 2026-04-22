using System;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSplitMepCurve.Models;

namespace RevitSplitMepCurve.ViewModels;

internal class MainViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;

    private string _errorText;
    private string _saveProperty;

    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService) {
        _pluginConfig = pluginConfig ?? throw new ArgumentNullException(nameof(pluginConfig));
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }

    public ICommand AcceptViewCommand { get; }

    public string ErrorText {
        get => _errorText;
        private set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public string SaveProperty {
        get => _saveProperty;
        set => RaiseAndSetIfChanged(ref _saveProperty, value);
    }

    private void LoadView() {
        LoadConfig();
    }

    private void AcceptView() {
        SaveConfig();
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
        var setting = _pluginConfig.GetSettings(_revitRepository.Document);

        SaveProperty = setting?.SaveProperty ?? _localizationService.GetLocalizedString("MainWindow.Hello");
    }

    private void SaveConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                      ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.SaveProperty = SaveProperty;
        _pluginConfig.SaveProjectConfig();
    }
}
