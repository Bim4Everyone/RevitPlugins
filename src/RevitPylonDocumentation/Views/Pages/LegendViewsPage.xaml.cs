using dosymep.SimpleServices;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Views.Pages;
/// <summary>
/// Логика взаимодействия для LegendViewsPage.xaml
/// </summary>
internal partial class LegendViewsPage {
    public LegendViewsPage() { }

    public LegendViewsPage(MainViewModel viewModel,
        ILoggerService loggerService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        InitializeComponent();
        DataContext = viewModel;
    }
}
