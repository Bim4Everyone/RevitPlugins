using System.Windows;
using System.Windows.Controls.Primitives;

using dosymep.SimpleServices;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Views.Pages;

internal partial class GeneralPage {
    public GeneralPage() { }

    public GeneralPage(MainViewModel viewModel,
        ILoggerService loggerService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        var window = Window.GetWindow(this) as MainWindow;
        if(window != null) {
            window.DialogResult = false;
        }
    }

    private void UniformGrid_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e) {
        UniformGrid uniformGrid = sender as UniformGrid;
        if(uniformGrid is null) { return; }

        if(uniformGrid.ActualWidth > 500) {
            uniformGrid.Columns = 2;
        } else {
            uniformGrid.Columns = 1;
        }
    }
}
