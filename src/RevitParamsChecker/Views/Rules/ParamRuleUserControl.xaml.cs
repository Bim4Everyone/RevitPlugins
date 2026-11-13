using dosymep.SimpleServices;

namespace RevitParamsChecker.Views.Rules;

internal partial class ParamRuleUserControl {
    public ParamRuleUserControl()
        : base() {
        InitializeComponent();
    }

    public ParamRuleUserControl(
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
