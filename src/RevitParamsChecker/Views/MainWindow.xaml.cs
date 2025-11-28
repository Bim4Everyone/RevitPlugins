using System.Windows;
using System.Windows.Threading;

using dosymep.SimpleServices;

using RevitParamsChecker.Views.Dashboard;

using Wpf.Ui.Abstractions;

namespace RevitParamsChecker.Views;

internal partial class MainWindow {
    public MainWindow(
        INavigationViewPageProvider navigationViewPageProvider,
        ILoggerService loggerService,
        ISerializationService serializationService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(
            loggerService,
            serializationService,
            languageService,
            localizationService,
            uiThemeService,
            themeUpdaterService) {
        InitializeComponent();
        _rootNavigationView.SetPageProviderService(navigationViewPageProvider);
        Dispatcher.BeginInvoke(
            DispatcherPriority.Loaded,
            () => _rootNavigationView.Navigate(typeof(DashboardPage)));
    }

    public override string PluginName => nameof(RevitParamsChecker);

    public override string ProjectConfigName => nameof(MainWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
