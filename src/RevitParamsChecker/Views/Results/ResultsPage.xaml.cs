using dosymep.SimpleServices;

namespace RevitParamsChecker.Views.Results;

public partial class ResultsPage {
    public ResultsPage() {
        InitializeComponent();
    }

    public ResultsPage(
        ILoggerService loggerService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService, languageService, localizationService, uiThemeService, themeUpdaterService) {
        InitializeComponent();
    }
}
