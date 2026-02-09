using dosymep.SimpleServices;

using RevitPylonDocumentation.Models;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Views.Pages;
/// <summary>
/// Логика взаимодействия для DispatcherParamsPage.xaml
/// </summary>
internal partial class DispatcherParamsPage {
    public DispatcherParamsPage() { }

    public DispatcherParamsPage(MainViewModel viewModel,
        ILoggerService loggerService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void UniformGrid_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e) {
        UniformGridHelper.HandleSizeChanged(sender);
    }
}
