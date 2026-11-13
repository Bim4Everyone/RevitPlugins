using dosymep.SimpleServices;

namespace RevitParamsChecker.Views.Filtration;

public partial class FiltrationPage {
    public FiltrationPage() {
        InitializeComponent();
    }

    public FiltrationPage(
        ILoggerService loggerService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService, languageService, localizationService, uiThemeService, themeUpdaterService) {
        InitializeComponent();
    }
}
