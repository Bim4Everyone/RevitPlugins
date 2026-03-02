using dosymep.SimpleServices;

namespace RevitParamsChecker.Views.Results;

internal partial class ReadonlyRuleControl {
    public ReadonlyRuleControl() {
        InitializeComponent();
    }

    public ReadonlyRuleControl(
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
