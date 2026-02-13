using dosymep.SimpleServices;

namespace RevitParamsChecker.Views.Rules;

internal partial class RuleUserControl {
    public RuleUserControl()
        : base() {
        InitializeComponent();
    }

    public RuleUserControl(
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
