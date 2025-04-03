using dosymep.SimpleServices;

using RevitPlatformSettings.ViewModels.Settings;

namespace RevitPlatformSettings.Views.Pages;

internal partial class AboutSettingsPage {
    public AboutSettingsPage() { }

    public AboutSettingsPage(AboutSettingsViewModel viewModel,
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
