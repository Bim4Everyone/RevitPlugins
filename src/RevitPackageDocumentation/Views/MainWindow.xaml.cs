using System.Windows;
using System.Windows.Input;

using dosymep.SimpleServices;

namespace RevitPackageDocumentation.Views;

/// <summary>
/// Класс главного окна плагина.
/// </summary>
public partial class MainWindow {
    /// <summary>
    /// Инициализирует главное окно плагина.
    /// </summary>
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

    /// <summary>
    /// Наименование плагина.
    /// </summary>
    /// <remarks>
    /// Используется для сохранения положения окна.
    /// </remarks>
    public override string PluginName => nameof(RevitPackageDocumentation);

    /// <summary>
    /// Наименование файла конфигурации.
    /// </summary>
    /// <remarks>
    /// Используется для сохранения положения окна.
    /// </remarks>
    public override string ProjectConfigName => nameof(MainWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }

    private void ComponentTypesComboBox_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
        if(!ComponentTypesComboBox.IsDropDownOpen) {
            ComponentTypesComboBox.IsDropDownOpen = true;
            e.Handled = true;
        }
    }

    private void SheetSetParamTypesComboBox_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
        if(!SheetSetParamTypesComboBox.IsDropDownOpen) {
            SheetSetParamTypesComboBox.IsDropDownOpen = true;
            e.Handled = true;
        }
    }
}
