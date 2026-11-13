using System.Windows;
using System.Windows.Threading;

using dosymep.SimpleServices;
using Wpf.Ui.Abstractions;

namespace RevitDeclarations.Views;
public partial class PublicAreasMainWindow {
    public PublicAreasMainWindow(INavigationViewPageProvider navigationViewPageProvider,
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
            _rootNavigationView.Navigate(typeof(DeclarationPublicAreasPage));
        });
    }

    public override string PluginName => nameof(RevitDeclarations);
    public override string ProjectConfigName => nameof(ApartmentsMainWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
