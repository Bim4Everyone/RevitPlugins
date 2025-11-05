using dosymep.SimpleServices;

namespace RevitOpeningPlacement.Views;
public partial class SettingView {
    public SettingView() : base() {
        InitializeComponent();
    }

    public SettingView(
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
