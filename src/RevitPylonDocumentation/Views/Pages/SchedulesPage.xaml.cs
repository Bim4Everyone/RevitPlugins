using System.Windows.Controls.Primitives;

using dosymep.SimpleServices;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Views.Pages;
/// <summary>
/// Логика взаимодействия для SchedulesPage.xaml
/// </summary>
internal partial class SchedulesPage {
    public SchedulesPage() { }

    public SchedulesPage(MainViewModel viewModel,
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
        UniformGrid uniformGrid = sender as UniformGrid;
        if(uniformGrid is null) { return; }

        if(uniformGrid.ActualWidth > 1300) {
            uniformGrid.Columns = 3;
        } else if(uniformGrid.ActualWidth > 900) {
            uniformGrid.Columns = 2;
        } else {
            uniformGrid.Columns = 1;
        }
    }
}
