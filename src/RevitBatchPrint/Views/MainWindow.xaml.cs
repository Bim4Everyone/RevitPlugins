using System.Windows;

using dosymep.SimpleServices;

namespace RevitBatchPrint.Views;

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

#if REVIT_2022_OR_GREATER
    public bool SupportExport => true;
#else
    public bool SupportExport => false;
#endif

    /// <summary>
    /// Наименование плагина.
    /// </summary>
    /// <remarks>
    /// Используется для сохранения положения окна.
    /// </remarks>
    public override string PluginName => nameof(RevitBatchPrint);

    /// <summary>
    /// Наименование файла конфигурации.
    /// </summary>
    /// <remarks>
    /// Используется для сохранения положения окна.
    /// </remarks>
    public override string ProjectConfigName => nameof(MainWindow);

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
