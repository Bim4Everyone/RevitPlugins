using dosymep.SimpleServices;

namespace RevitOpeningPlacement.Views.Settings;
public partial class StructureCategoryView {
    public StructureCategoryView() : base() {
        InitializeComponent();
    }

    public StructureCategoryView(
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
