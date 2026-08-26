using dosymep.SimpleServices;

using RevitLintelsManager.ViewModels;

namespace RevitLintelsManager.Views.Pages;

internal partial class FamilySettingsPage {
    public FamilySettingsPage(SettingsViewModel viewModel, ILoggerService loggerService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService) 
        : base(loggerService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        InitializeComponent();
        DataContext = viewModel.FamilySettingsViewModel;
    }
}
