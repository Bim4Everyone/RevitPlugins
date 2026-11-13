using dosymep.SimpleServices;

namespace RevitParamsChecker.Views.Results;

internal partial class CheckResultControl {
    public CheckResultControl() {
        InitializeComponent();
    }

    public CheckResultControl(
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
