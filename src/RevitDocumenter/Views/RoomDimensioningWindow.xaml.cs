using System.Windows;
using System.Windows.Input;

using dosymep.SimpleServices;

namespace RevitDocumenter.Views;

/// <summary>
/// Окно образмеривания помещений.
/// </summary>
public partial class RoomDimensioningWindow {
    public RoomDimensioningWindow(
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

    /// <summary>
    /// Наименование плагина. Используется для сохранения положения окна.
    /// </summary>
    public override string PluginName => nameof(RevitDocumenter);

    /// <summary>
    /// Наименование файла конфигурации. Используется для сохранения положения окна.
    /// </summary>
    public override string ProjectConfigName => nameof(RoomDimensioningWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e) {
        if(e.Key == Key.Escape) {
            Close();
        }
        if(e.Key == Key.Enter && OkButton.Command.CanExecute(null)) {
            ButtonOk_Click(sender, e);
            OkButton.Command.Execute(null);
        }
    }
}
