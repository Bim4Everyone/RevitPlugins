using System.Windows.Controls;

using dosymep.SimpleServices;

using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Views;
internal partial class ParamsApartmentsTabItem {
    public ParamsApartmentsTabItem(ApartmentsMainVM viewModel, ILoggerService loggerService,
                                   ILanguageService languageService, ILocalizationService localizationService,
                                   IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService) 
            : base(loggerService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        InitializeComponent();
        DataContext = viewModel.ParametersViewModel;
    }
}
