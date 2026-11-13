using dosymep.SimpleServices;

namespace RevitOpeningPlacement.Views.Settings.Ar;
public partial class ArPlacementSettingsPage {
    public ArPlacementSettingsPage() {
        InitializeComponent();
    }

    public ArPlacementSettingsPage(
        ILoggerService loggerService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService, languageService, localizationService, uiThemeService, themeUpdaterService) {
        InitializeComponent();
    }
}
