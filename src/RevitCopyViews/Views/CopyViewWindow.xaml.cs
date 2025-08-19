using System.Windows;

using dosymep.SimpleServices;

namespace RevitCopyViews.Views;

/// <summary>
///     Interaction logic for CopyViewWindow.xaml
/// </summary>
public partial class CopyViewWindow {
    public CopyViewWindow(
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
    }

    public override string PluginName => nameof(RevitCopyViews);
    public override string ProjectConfigName => nameof(CopyViewWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
