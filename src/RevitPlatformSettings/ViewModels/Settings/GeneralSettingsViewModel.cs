using System.Globalization;

using DevExpress.Mvvm.UI;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Bim4Everyone.SimpleServices.Configuration;
using dosymep.SimpleServices;

namespace RevitPlatformSettings.ViewModels.Settings;

internal sealed class GeneralSettingsViewModel : SettingsViewModel {
    private readonly ILanguageService _languageService;
    private readonly IUIThemeService _uiThemeService;
    private readonly IPlatformSettingsService _platformSettingsService;
    
    private bool? _notificationIsActive;
    private int? _notificationVisibleMaxCount;
    
    private NotificationScreen? _notificationScreen;
    private NotificationPosition? _notificationPosition;

    public GeneralSettingsViewModel(
        IPlatformSettingsService platformSettingsService,
        IUIThemeService uiThemeService,
        ILanguageService languageService) {
        _platformSettingsService = platformSettingsService;
        _uiThemeService = uiThemeService;
        _languageService = languageService;

        CorpName = _platformSettingsService.CorpSettings.Name;
        SettingsPath = _platformSettingsService.CorpSettings.SettingsPath;
        SettingsImagePath = _platformSettingsService.CorpSettings.ImagePath;

        HostTheme = _uiThemeService.HostTheme;
        HostLanguage = _languageService.HostLanguage;

        NotificationIsActive = _platformSettingsService.NotificationSettings.IsActive;
        NotificationScreen = _platformSettingsService.NotificationSettings.NotificationScreen;
        NotificationPosition = _platformSettingsService.NotificationSettings.NotificationPosition;
        NotificationVisibleMaxCount = _platformSettingsService.NotificationSettings.NotificationVisibleMaxCount;
    }

    public string CorpName { get; }
    public string SettingsPath { get; }
    public string SettingsImagePath { get; }

    public UIThemes HostTheme { get; }
    public CultureInfo HostLanguage { get; }

    public bool? NotificationIsActive {
        get => _notificationIsActive;
        set => RaiseAndSetIfChanged(ref _notificationIsActive, value);
    }

    public NotificationScreen? NotificationScreen {
        get => _notificationScreen;
        set => RaiseAndSetIfChanged(ref _notificationScreen, value);
    }

    public NotificationPosition? NotificationPosition {
        get => _notificationPosition;
        set => RaiseAndSetIfChanged(ref _notificationPosition, value);
    }

    public int? NotificationVisibleMaxCount {
        get => _notificationVisibleMaxCount;
        set => RaiseAndSetIfChanged(ref _notificationVisibleMaxCount, value);
    }

    public override void SaveSettings() {
        NotificationSettings notificationSettings
            = _platformSettingsService.NotificationSettings;

        notificationSettings.IsActive = NotificationIsActive;
        notificationSettings.NotificationScreen = NotificationScreen;
        notificationSettings.NotificationPosition = NotificationPosition;
        notificationSettings.NotificationVisibleMaxCount = NotificationVisibleMaxCount;
    }
}
