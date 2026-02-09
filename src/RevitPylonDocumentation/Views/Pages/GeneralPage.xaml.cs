using System.Windows;

using dosymep.SimpleServices;

using RevitPylonDocumentation.Models;
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
        UniformGridHelper.HandleSizeChanged(sender);
    }
}
