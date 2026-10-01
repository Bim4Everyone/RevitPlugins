using dosymep.SimpleServices;

namespace RevitOpeningPlacement.Views.Settings.Kr;
public partial class KrNavigatorSettingsPage {
    public KrNavigatorSettingsPage() {
        InitializeComponent();
    }

    public KrNavigatorSettingsPage(
        ILoggerService loggerService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService, languageService, localizationService, uiThemeService, themeUpdaterService) {
        InitializeComponent();
    }
}
