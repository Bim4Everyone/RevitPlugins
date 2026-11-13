using dosymep.SimpleServices;

namespace RevitOpeningPlacement.Views.Settings.Mep;
public partial class MepNavigatorSettingsPage {
    public MepNavigatorSettingsPage() {
        InitializeComponent();
    }

    public MepNavigatorSettingsPage(
        ILoggerService loggerService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService, languageService, localizationService, uiThemeService, themeUpdaterService) {
        InitializeComponent();
    }
}
