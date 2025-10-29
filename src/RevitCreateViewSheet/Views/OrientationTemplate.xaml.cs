using dosymep.SimpleServices;

namespace RevitCreateViewSheet.Views;
public partial class OrientationTemplate {
    public OrientationTemplate() : base() {
        InitializeComponent();
    }

    public OrientationTemplate(
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
