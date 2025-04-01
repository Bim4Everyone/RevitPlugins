using System.Windows;

using dosymep.SimpleServices;

using RevitCreateViewSheet.Resources;

namespace RevitCreateViewSheet.Views;

public partial class MainWindow {
    public MainWindow(
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
        Resources.Add(nameof(BoolToLocalizedStringConverter), new BoolToLocalizedStringConverter(localizationService));
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitCreateViewSheet);
    public override string ProjectConfigName => nameof(MainWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}

