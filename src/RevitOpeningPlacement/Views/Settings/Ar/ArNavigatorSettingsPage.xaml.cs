using dosymep.SimpleServices;

namespace RevitOpeningPlacement.Views.Settings.Ar;
public partial class ArNavigatorSettingsPage {
    public ArNavigatorSettingsPage() {
        InitializeComponent();
    }

    public ArNavigatorSettingsPage(
        ILoggerService loggerService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService, languageService, localizationService, uiThemeService, themeUpdaterService) {
        InitializeComponent();
    }
}
