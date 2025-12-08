using System.Windows.Threading;

using dosymep.SimpleServices;

using RevitPylonDocumentation.Views.Pages;

using Wpf.Ui.Abstractions;

namespace RevitPylonDocumentation.Views;
public partial class MainWindow {
    public MainWindow(
        INavigationViewPageProvider navigationViewPageProvider, 
        ILoggerService loggerService,
        ISerializationService serializationService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
            serializationService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        InitializeComponent();
        _rootNavigationView.SetPageProviderService(navigationViewPageProvider);
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () => {
            _rootNavigationView.Navigate(typeof(GeneralPage));
        });
    }

    public override string PluginName => nameof(RevitPylonDocumentation);
    public override string ProjectConfigName => nameof(MainWindow);
}
