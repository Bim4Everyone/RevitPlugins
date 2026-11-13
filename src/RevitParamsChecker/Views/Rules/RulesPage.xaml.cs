using dosymep.SimpleServices;

namespace RevitParamsChecker.Views.Rules;

public partial class RulesPage {
    public RulesPage() {
        InitializeComponent();
    }

    public RulesPage(
        ILoggerService loggerService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService, languageService, localizationService, uiThemeService, themeUpdaterService) {
        InitializeComponent();
    }
}
