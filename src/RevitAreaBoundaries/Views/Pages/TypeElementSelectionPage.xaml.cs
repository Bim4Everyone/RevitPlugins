using dosymep.SimpleServices;

using RevitAreaBoundaries.ViewModels;

namespace RevitAreaBoundaries.Views.Pages;

internal partial class TypeElementSelectionPage {
    public TypeElementSelectionPage(MainViewModel viewModel, ILoggerService loggerService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService) 
        : base(loggerService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        InitializeComponent();
        DataContext = viewModel.TypeElementSelectionViewModel;
    }
}
