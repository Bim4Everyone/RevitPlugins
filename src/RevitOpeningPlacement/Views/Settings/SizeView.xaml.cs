using dosymep.SimpleServices;

namespace RevitOpeningPlacement.Views.Settings;

public partial class SizeView {
    public SizeView() : base() {
        InitializeComponent();
    }

    public SizeView(
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
