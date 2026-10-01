using dosymep.SimpleServices;

namespace RevitOpeningPlacement.Views.Settings.Controls;
public partial class IncomingStatusesControl {
    public IncomingStatusesControl() : base() {
        InitializeComponent();
    }

    public IncomingStatusesControl(
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
