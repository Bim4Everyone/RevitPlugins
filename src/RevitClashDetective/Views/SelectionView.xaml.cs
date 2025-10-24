using dosymep.SimpleServices;

namespace RevitClashDetective.Views;
public partial class SelectionView {

    public SelectionView() : base() {
        InitializeComponent();
    }

    public SelectionView(
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
