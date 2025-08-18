using System.Windows;

using dosymep.SimpleServices;

namespace RevitCreatingFiltersByValues.Views;
public partial class MainWindow {
    public MainWindow(
        ILoggerService loggerService,
        ISerializationService serializationService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
            serializationService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        InitializeComponent();
    }


    public override string PluginName => nameof(RevitCreatingFiltersByValues);
    public override string ProjectConfigName => nameof(MainWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }

    private void WindowLoaded(object sender, RoutedEventArgs e) {
        expander.MaxHeight = window.ActualHeight * 0.8;
    }
}
