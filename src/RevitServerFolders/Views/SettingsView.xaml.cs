using dosymep.SimpleServices;

namespace RevitServerFolders.Views;
public partial class SettingsView {
    public SettingsView() : base() {
        InitializeComponent();
    }


    public SettingsView(
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
