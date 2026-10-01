using dosymep.SimpleServices;

namespace RevitOpeningPlacement.Views.Settings.Kr;
public partial class KrPlacementSettingsPage {
    public KrPlacementSettingsPage() {
        InitializeComponent();
    }

    public KrPlacementSettingsPage(
        ILoggerService loggerService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService, languageService, localizationService, uiThemeService, themeUpdaterService) {
        InitializeComponent();
    }
}
