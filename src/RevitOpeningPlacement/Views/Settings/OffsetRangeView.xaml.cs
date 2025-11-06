using dosymep.SimpleServices;

namespace RevitOpeningPlacement.Views.Settings;
public partial class OffsetRangeView {
    public OffsetRangeView() : base() {
        InitializeComponent();
    }

    public OffsetRangeView(
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
