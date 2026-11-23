using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitLintelsManager.Models.Configs;
using RevitLintelsManager.Models.Rules;
using RevitLintelsManager.Models.Settings;
using RevitLintelsManager.Services;

namespace RevitLintelsManager.ViewModels;

internal class SettingsViewModel : BaseViewModel {
    public event Action CloseRequested;

    private readonly IPluginConfigRepository _configRepository;
    private readonly ILocalizationService _localization;
    private readonly IMessageBoxService _messages;
    private readonly ILintelConfigRuleResolver _ruleResolver;
    private readonly LintelManagerSettings _sourceSettings;
    private string _errorText;
    private bool _hasFamilySettingsErrors;
    private bool _hasPlacementSettingsErrors;
    private bool _hasGeneralSettingsErrors;
    private bool _isLoaded;

    /// <summary>Создаёт SettingsViewModel с переданными зависимостями.</summary>
    public SettingsViewModel(ILocalizationService localization, IPluginConfigRepository configRepository,
        ILintelConfigRuleResolver resolver, IMessageBoxService messages, ISettingsCommandFactory commandFactory,
        FamilySettingsViewModel familySettings, GeneralSettingsViewModel generalSettings,
        PlacementSettingsViewModel placementSettings, LintelManagerSettings sourceSettings) {
        _configRepository = configRepository;
        _localization = localization;
        _messages = messages;
        _ruleResolver = resolver;
        _sourceSettings = sourceSettings;
        FamilySettingsViewModel = familySettings;
        GeneralSettingsViewModel = generalSettings;
        PlacementSettingsViewModel = placementSettings;
        LoadViewCommand = commandFactory.Create(LoadView);
        AcceptViewCommand = commandFactory.Create(AcceptView, CanAcceptView);
        CanAcceptView();
    }

    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }
    public FamilySettingsViewModel FamilySettingsViewModel { get; }
    public PlacementSettingsViewModel PlacementSettingsViewModel { get; }
    public GeneralSettingsViewModel GeneralSettingsViewModel { get; }

    public string ErrorText {
        get => _errorText;
        private set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public bool HasFamilySettingsErrors {
        get => _hasFamilySettingsErrors;
        private set => RaiseAndSetIfChanged(ref _hasFamilySettingsErrors, value);
    }

    public bool HasPlacementSettingsErrors {
        get => _hasPlacementSettingsErrors;
        private set => RaiseAndSetIfChanged(ref _hasPlacementSettingsErrors, value);
    }

    public bool HasGeneralSettingsErrors {
        get => _hasGeneralSettingsErrors;
        private set => RaiseAndSetIfChanged(ref _hasGeneralSettingsErrors, value);
    }

    // Однократно показывает ошибки загрузки файлов правил.
    private void LoadView() {
        if(_isLoaded) {
            return;
        }
        _isLoaded = true;
        if(_ruleResolver.LoadErrors.Count > 0) {
            _messages.Show(_localization.GetLocalizedString("Config.LoadErrors") + Environment.NewLine
                + string.Join(Environment.NewLine, _ruleResolver.LoadErrors),
                _localization.GetLocalizedString("Common.Error"), MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // Проверяет вкладки и доступность сохранения настроек.
    private bool CanAcceptView() {
        string familyError = FamilySettingsViewModel.GetErrorText();
        string placementError = PlacementSettingsViewModel.GetErrorText();
        string generalError = GeneralSettingsViewModel.GetErrorText();
        HasFamilySettingsErrors = familyError is not null;
        HasPlacementSettingsErrors = placementError is not null;
        HasGeneralSettingsErrors = generalError is not null;
        ErrorText = familyError ?? placementError ?? generalError;
        return ErrorText is null;
    }

    // Сохраняет правила и настройки с восстановлением файлов при ошибке.
    private void AcceptView() {
        if(!CanAcceptView()) {
            return;
        }
        var settings = GeneralSettingsViewModel.BuildSettings(_sourceSettings);
        settings.LintelFamilySettings = FamilySettingsViewModel.GetLintelSettings();
        settings.OpeningFamilySettings = FamilySettingsViewModel.GetOpeningSettings();
        settings.LintelConfigRuleName = PlacementSettingsViewModel.SelectedConfig.Name;
        settings.LintelWallSettings = PlacementSettingsViewModel.GetWallSettings();
        settings.LintelFixParamName = GeneralSettingsViewModel.SelectedFixParameter.RevitParam.Name;
        settings.MinimalHeightAboveOpeningMm = GeneralSettingsViewModel.GetMinimalHeight();
        settings.PhaseName = GeneralSettingsViewModel.SelectedPhase.Name;
        try {
            SettingsFileTransaction.Execute(PlacementSettingsViewModel.GetPendingPaths()
                .Append(_configRepository.ConfigPath), () => {
                    PlacementSettingsViewModel.SaveConfigs();
                    _configRepository.SaveSettings(settings);
                });
        } catch(Exception ex) {
            _messages.Show(ex.Message, _localization.GetLocalizedString("Common.Error"),
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        CloseRequested?.Invoke();
    }
}
