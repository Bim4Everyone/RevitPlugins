using dosymep.SimpleServices;

namespace RevitParamsChecker.Views.Checks;

internal partial class ChecksControl {
    public ChecksControl()
        : base() {
        InitializeComponent();
    }

    public ChecksControl(
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
