using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using RevitDeclarations.ViewModels;


namespace RevitDeclarations.Views;
internal partial class PrioritiesTabItem {
    public PrioritiesTabItem(MainViewModel viewModel, ILoggerService loggerService,
                            ILanguageService languageService, ILocalizationService localizationService,
                            IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService) : base(loggerService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        InitializeComponent();
        DataContext = viewModel.PrioritiesViewModel;
    }
}
