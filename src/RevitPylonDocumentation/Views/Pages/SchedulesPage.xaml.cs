using System.Windows;

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


    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        var window = Window.GetWindow(this) as MainWindow;
        if(window != null) {
            window.DialogResult = true;
        }
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        var window = Window.GetWindow(this) as MainWindow;
        if(window != null) {
            window.DialogResult = false;
        }
    }
}
