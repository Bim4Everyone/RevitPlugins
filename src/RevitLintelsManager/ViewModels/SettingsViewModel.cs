using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitLintelsManager.Models;
using RevitLintelsManager.Models.Configs;
using RevitLintelsManager.Models.Rules;
using RevitLintelsManager.Models.Settings;
using RevitLintelsManager.Services;

namespace RevitLintelsManager.ViewModels;

internal class SettingsViewModel : BaseViewModel {
    public event Action CloseRequested;

    private readonly PluginConfigRepository _configRepository;
    private readonly ILocalizationService _localization;
    private readonly IMessageBoxService _messages;
    private readonly LintelConfigRuleResolver _ruleResolver;
    private string _errorText;
    private bool _hasFamilySettingsErrors;
    private bool _hasPlacementSettingsErrors;
    private bool _hasGeneralSettingsErrors;
    private bool _isLoaded;

    public SettingsViewModel(RevitRepository repository, SystemPluginConfig systemConfig,
        ILocalizationService localization, PluginConfigRepository configRepository, ParamService paramService,
        LintelConfigRuleStorage storage, LintelConfigRuleSerializer serializer, LintelConfigRuleResolver resolver,
        SettingsDialogService dialogs, IOpenFileDialogService openDialog, ISaveFileDialogService saveDialog,
        IMessageBoxService messages, SettingsCommandFactory commandFactory) {
        _configRepository = configRepository;
        _localization = localization;
        _messages = messages;
        _ruleResolver = resolver;
        var savedSettings = configRepository.LoadSettings();
        var settings = savedSettings ?? configRepository.GetDefaultLintelManagerSettings();
        FamilySettingsViewModel = new FamilySettingsViewModel(repository, systemConfig, localization, settings,
            savedSettings is not null, commandFactory);
        GeneralSettingsViewModel = new GeneralSettingsViewModel(repository, paramService, localization,
            FamilySettingsViewModel, settings, commandFactory);
        PlacementSettingsViewModel = new PlacementSettingsViewModel(repository, systemConfig, localization,
            storage, serializer, resolver, dialogs, openDialog, saveDialog, messages,
            FamilySettingsViewModel, GeneralSettingsViewModel, settings, commandFactory);
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

    private void AcceptView() {
        if(!CanAcceptView()) {
            return;
        }
        var settings = new LintelManagerSettings {
            LintelFamilySettings = FamilySettingsViewModel.GetLintelSettings(),
            OpeningFamilySettings = FamilySettingsViewModel.GetOpeningSettings(),
            LintelConfigRuleName = PlacementSettingsViewModel.SelectedConfig.Name,
            LintelWallSettings = PlacementSettingsViewModel.GetWallSettings(),
            LintelFixParamName = GeneralSettingsViewModel.SelectedFixParameter.RevitParam.Name,
            MinimalHeightAboveOpeningMm = GeneralSettingsViewModel.GetMinimalHeight(),
            PhaseName = GeneralSettingsViewModel.SelectedPhase.Name,
            StructureWallTypeNames = GeneralSettingsViewModel.GetStructureWallNames()
        };
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
