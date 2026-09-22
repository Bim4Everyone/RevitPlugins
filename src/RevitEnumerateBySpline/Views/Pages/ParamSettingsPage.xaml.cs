using dosymep.SimpleServices;

using RevitEnumerateBySpline.ViewModels;

namespace RevitEnumerateBySpline.Views.Pages;

internal partial class ParamSettingsPage {
    public ParamSettingsPage(MainViewModel viewModel, ILoggerService loggerService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService) 
        : base(loggerService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        InitializeComponent();
        DataContext = viewModel.CommonSettingsViewModel;
    }
}

