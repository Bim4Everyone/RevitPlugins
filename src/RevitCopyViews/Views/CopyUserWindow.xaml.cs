using System.Windows;

using dosymep.SimpleServices;

namespace RevitCopyViews.Views;

/// <summary>
///     Interaction logic for CopyUserView.xaml
/// </summary>
public partial class CopyUserWindow {
    public CopyUserWindow(
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
    public override string ProjectConfigName => nameof(CopyUserWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
