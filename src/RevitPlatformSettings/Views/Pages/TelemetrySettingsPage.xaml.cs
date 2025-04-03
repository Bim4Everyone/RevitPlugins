using dosymep.SimpleServices;

using RevitPlatformSettings.ViewModels.Settings;

namespace RevitPlatformSettings.Views.Pages;

internal partial class TelemetrySettingsPage {
    public TelemetrySettingsPage() { }

    public TelemetrySettingsPage(TelemetrySettingsViewModel viewModel,
        ILoggerService loggerService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        InitializeComponent();
        DataContext = viewModel;
    }
}
