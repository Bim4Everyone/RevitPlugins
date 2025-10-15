using dosymep.SimpleServices;

namespace RevitClashDetective.Views;
public partial class CriterionView {
    public CriterionView() : base() {
        InitializeComponent();
    }

    public CriterionView(
        ILoggerService loggerService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
            languageService,
            localizationService,
            uiThemeService,
            themeUpdaterService) {
        InitializeComponent();
    }
}
