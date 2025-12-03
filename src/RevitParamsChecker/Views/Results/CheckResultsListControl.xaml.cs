using dosymep.SimpleServices;

namespace RevitParamsChecker.Views.Results;

internal partial class CheckResultsListControl {
    public CheckResultsListControl() {
        InitializeComponent();
    }

    public CheckResultsListControl(
        ILoggerService loggerService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(
            loggerService,
            languageService,
            localizationService,
            uiThemeService,
            themeUpdaterService) {
        InitializeComponent();
    }
}
