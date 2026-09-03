using System.Windows;

using dosymep.SimpleServices;

namespace RevitClassifierParameters.Views;

/// <summary>
/// Класс окна отчёта плагина.
/// </summary>
public partial class ReportV {
    /// <summary>
    /// Инициализирует окно отчёта плагина.
    /// </summary>
    public ReportV(
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
    public override string PluginName => nameof(RevitClassifierParameters);

    /// <summary>
    /// Наименование файла конфигурации.
    /// </summary>
    /// <remarks>
    /// Используется для сохранения положения окна.
    /// </remarks>
    public override string ProjectConfigName => nameof(ReportV);

    private void ButtonForHide_Click(object sender, RoutedEventArgs e) {
        Hide();
    }
}
