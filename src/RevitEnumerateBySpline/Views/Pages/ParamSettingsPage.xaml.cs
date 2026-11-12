using dosymep.SimpleServices;

using RevitEnumerateBySpline.ViewModels;

namespace RevitEnumerateBySpline.Views.Pages;

/// <summary>Страница параметров нумерации.</summary>
internal partial class ParamSettingsPage {
    /// <summary>Создаёт страницу параметров нумерации.</summary>
    public ParamSettingsPage(MainViewModel viewModel, ILoggerService loggerService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService) 
        : base(loggerService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        InitializeComponent();
        DataContext = viewModel;
    }
}

