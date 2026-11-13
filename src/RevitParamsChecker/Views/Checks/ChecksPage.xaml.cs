using dosymep.SimpleServices;

namespace RevitParamsChecker.Views.Checks;

public partial class ChecksPage {
    public ChecksPage() {
        InitializeComponent();
    }

    public ChecksPage(
        ILoggerService loggerService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService, languageService, localizationService, uiThemeService, themeUpdaterService) {
        InitializeComponent();
    }
}
