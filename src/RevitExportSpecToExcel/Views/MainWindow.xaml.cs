using System.Windows;

using dosymep.SimpleServices;

using RevitExportSpecToExcel.ViewModels;

using Wpf.Ui.Controls;

namespace RevitExportSpecToExcel.Views;

/// <summary>
/// Класс главного окна плагина.
/// </summary>
public partial class MainWindow {
    /// <summary>
    /// Иницализирует главное окно плагина.
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
    public override string PluginName => nameof(RevitExportSpecToExcel);
    
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

    private void CheckBox_Checked(object sender, RoutedEventArgs e) {
        ChangeSelected(true);
    }

    private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
        ChangeSelected(false);
    }

    private void ChangeSelected(bool state) {
        var listBox = (DataGrid) FindName("Schedules");
        var schedules = listBox.SelectedItems;
        foreach(ScheduleViewModel schedule in schedules) {
            schedule.IsChecked = state;
        }
    }
}
