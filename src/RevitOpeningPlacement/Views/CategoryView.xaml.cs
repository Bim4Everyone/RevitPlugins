using dosymep.SimpleServices;

namespace RevitOpeningPlacement.Views;
public partial class CategoryView {
    public CategoryView() : base() {
        InitializeComponent();
    }
    public CategoryView(
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
