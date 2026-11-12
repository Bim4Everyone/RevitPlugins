using dosymep.SimpleServices;

using RevitEnumerateBySpline.ViewModels;

namespace RevitEnumerateBySpline.Views.Pages;

/// <summary>Страница выбора помещений и кривых.</summary>
internal partial class CommonSettingsPage {
    /// <summary>Создаёт страницу выбора помещений и кривых.</summary>
    public CommonSettingsPage(MainViewModel viewModel, ILoggerService loggerService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService) 
        : base(loggerService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        InitializeComponent();
        DataContext = viewModel;
    }
}

