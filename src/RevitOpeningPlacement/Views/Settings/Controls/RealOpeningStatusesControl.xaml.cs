using dosymep.SimpleServices;

namespace RevitOpeningPlacement.Views.Settings.Controls;
public partial class RealOpeningStatusesControl {
    public RealOpeningStatusesControl() : base() {
        InitializeComponent();
    }

    public RealOpeningStatusesControl(
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
