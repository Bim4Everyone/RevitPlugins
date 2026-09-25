using System.Windows;
using System.Windows.Threading;

using dosymep.SimpleServices;

using RevitOpeningPlacement.Views.Settings.Ar;

using Wpf.Ui.Abstractions;

namespace RevitOpeningPlacement.Views.Settings;
public partial class ArSettingsWindow {
    public ArSettingsWindow(
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
            () => _rootNavigationView.Navigate(typeof(ArPlacementSettingsPage)));
    }

    public override string PluginName => nameof(RevitOpeningPlacement);
    public override string ProjectConfigName => nameof(ArSettingsWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
