using dosymep.SimpleServices;

namespace RevitOpeningPlacement.Views;
public partial class SetView {
    public SetView() : base() {
        InitializeComponent();
    }

    public SetView(
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
