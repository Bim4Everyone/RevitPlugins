using dosymep.SimpleServices;

using RevitMarkAllDocuments.ViewModels;

namespace RevitMarkAllDocuments.Views;

internal partial class MarkSettingsPage {
    public MarkSettingsPage(MainViewModel viewModel,
                            ILoggerService loggerService,
                            ILanguageService languageService,
                            ILocalizationService localizationService,
                            IUIThemeService uiThemeService,
                            IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
               languageService, localizationService,
               uiThemeService, themeUpdaterService) {
        InitializeComponent();
        DataContext = viewModel.MarkSettingsPageViewModel;
    }
}
