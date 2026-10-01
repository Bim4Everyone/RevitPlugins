using System.Windows;
using System.Windows.Threading;

using dosymep.SimpleServices;

using RevitOpeningPlacement.Views.Settings.Kr;

using Wpf.Ui.Abstractions;

namespace RevitOpeningPlacement.Views.Settings;
public partial class KrSettingsWindow {
    public KrSettingsWindow(
    INavigationViewPageProvider navigationViewPageProvider,
    ILoggerService loggerService,
    ISerializationService serializationService,
    ILanguageService languageService,
    ILocalizationService localizationService,
    IUIThemeService uiThemeService,
    IUIThemeUpdaterService themeUpdaterService)
    : base(loggerService,
        serializationService,
        languageService, localizationService,
        uiThemeService, themeUpdaterService) {
        InitializeComponent();
        _rootNavigationView.SetPageProviderService(navigationViewPageProvider);
        Dispatcher.BeginInvoke(
            DispatcherPriority.Loaded,
            () => _rootNavigationView.Navigate(typeof(KrPlacementSettingsPage)));
    }

    public override string PluginName => nameof(RevitOpeningPlacement);
    public override string ProjectConfigName => nameof(KrSettingsWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
