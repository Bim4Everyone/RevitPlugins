using dosymep.SimpleServices;

namespace RevitParamsChecker.Views.Dashboard;

public partial class DashboardPage {
    public DashboardPage() {
        InitializeComponent();
    }

    public DashboardPage(
        ILoggerService loggerService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService, languageService, localizationService, uiThemeService, themeUpdaterService) {
        InitializeComponent();
    }
}
