using System.Windows;

using dosymep.SimpleServices;

using Wpf.Ui;

namespace RevitGenLookupTables.Views;

/// <summary>
///     Класс главного окна плагина.
/// </summary>
public partial class MainWindow {
    /// <summary>
    ///     Иницализирует главное окно плагина.
    /// </summary>
    public MainWindow(
        ILoggerService loggerService,
        ISerializationService serializationService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService, 
        IContentDialogService contentDialogService)
        : base(
            loggerService,
            serializationService,
            languageService,
            localizationService,
            uiThemeService,
            themeUpdaterService) {
        InitializeComponent();
        contentDialogService.SetDialogHost(_rootContentDialog);
    }

    /// <summary>
    ///     Наименование плагина.
    /// </summary>
    /// <remarks>
    ///     Используется для сохранения положения окна.
    /// </remarks>
    public override string PluginName => nameof(RevitGenLookupTables);

    /// <summary>
    ///     Наименование файла конфигурации.
    /// </summary>
    /// <remarks>
    ///     Используется для сохранения положения окна.
    /// </remarks>
    public override string ProjectConfigName => nameof(MainWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
