using System.Windows;
using System.Windows.Input;

using dosymep.SimpleServices;

namespace RevitCopyViews.Views;

/// <summary>
///     Interaction logic for RenameViewWindow.xaml
/// </summary>
public partial class RenameViewWindow {
    public RenameViewWindow(
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
    public override string ProjectConfigName => nameof(RenameViewWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }

    private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
        DialogResult = false;
    }
}
