using System.Windows;

using dosymep.SimpleServices;

using Wpf.Ui;

namespace RevitServerFolders.Views;
public partial class MainWindow {
    public MainWindow(
    ILoggerService loggerService,
    ISerializationService serializationService,
    ILanguageService languageService,
    ILocalizationService localizationService,
    IUIThemeService uiThemeService,
    IUIThemeUpdaterService themeUpdaterService,
    IContentDialogService contentDialogService)
    : base(loggerService,
        serializationService,
        languageService, localizationService,
        uiThemeService, themeUpdaterService) {
        InitializeComponent();
        contentDialogService.SetDialogHost(_rootContentDialog);
    }

    public override string PluginName => nameof(RevitServerFolders);
    public override string ProjectConfigName => nameof(MainWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
