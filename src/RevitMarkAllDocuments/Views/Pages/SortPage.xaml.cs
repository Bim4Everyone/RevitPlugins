using dosymep.SimpleServices;

using RevitMarkAllDocuments.ViewModels;

namespace RevitMarkAllDocuments.Views;

internal partial class SortPage {
    public SortPage(MainViewModel viewModel,
                    ILoggerService loggerService,
                    ILanguageService languageService,
                    ILocalizationService localizationService,
                    IUIThemeService uiThemeService,
                    IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
               languageService, localizationService,
               uiThemeService, themeUpdaterService) {
        InitializeComponent();
        DataContext = viewModel.SortPageViewModel;
    }
}
