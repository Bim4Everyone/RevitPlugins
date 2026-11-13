using System;
using System.Windows;
using System.Windows.Threading;

using dosymep.SimpleServices;

using Ninject;

using RevitParamsChecker.Views.Checks;

using Wpf.Ui;
using Wpf.Ui.Abstractions;

namespace RevitParamsChecker.Views;

internal partial class MainWindow : IDisposable {
    private readonly IKernel _kernel;

    public MainWindow(
        IKernel kernel,
        IContentDialogService contentDialogService,
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
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        contentDialogService.SetDialogHost(_rootContentDialog);
        _rootNavigationView.SetPageProviderService(navigationViewPageProvider);
        Dispatcher.BeginInvoke(
            DispatcherPriority.Loaded,
            () => _rootNavigationView.Navigate(typeof(ChecksPage)));
    }

    public void Dispose() {
        _kernel?.Dispose();
    }

    public override string PluginName => nameof(RevitParamsChecker);

    public override string ProjectConfigName => nameof(MainWindow);

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        Close();
    }

    private void MainWindow_OnClosed(object sender, EventArgs e) {
        Dispose();
    }
}
