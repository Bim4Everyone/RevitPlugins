using dosymep.SimpleServices;

namespace RevitParamsChecker.Views.Rules;

internal partial class ParamsSetUserControl {
    public ParamsSetUserControl()
        : base() {
        InitializeComponent();
    }

    public ParamsSetUserControl(
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
