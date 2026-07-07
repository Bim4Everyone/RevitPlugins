using System.Windows;
using System.Windows.Threading;

using dosymep.SimpleServices;

using RevitAreaBoundaries.Views.Pages;

using Wpf.Ui.Abstractions;

namespace RevitAreaBoundaries.Views;

/// <summary>
/// Класс главного окна плагина.
/// </summary>
public partial class MainWindow {
    /// <summary>
    /// Иницализирует главное окно плагина.
    /// </summary>
    public MainWindow(INavigationViewPageProvider navigationViewPageProvider,
        ILoggerService loggerService,
        ISerializationService serializationService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService) 
        : base(loggerService,
        serializationService,
        languageService,
        localizationService,
        uiThemeService,
        themeUpdaterService) {
        InitializeComponent();
        
        _rootNavigationView.SetPageProviderService(navigationViewPageProvider);

        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () => {
            _rootNavigationView.Navigate(typeof(ViewPlanSelectionPage));
        });
    }

    
    public override string PluginName => nameof(RevitAreaBoundaries);
    
    public override string ProjectConfigName => nameof(MainWindow);
    
    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
