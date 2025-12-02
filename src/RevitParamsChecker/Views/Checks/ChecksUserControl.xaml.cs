using dosymep.SimpleServices;

namespace RevitParamsChecker.Views.Checks;

internal partial class ChecksUserControl {
    public ChecksUserControl()
        : base() {
        InitializeComponent();
    }

    public ChecksUserControl(
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
