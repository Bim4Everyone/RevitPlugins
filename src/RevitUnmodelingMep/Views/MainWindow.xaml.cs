using System.Windows;
using System.Windows.Controls;

using dosymep.SimpleServices;

using RevitUnmodelingMep.ViewModels;

namespace RevitUnmodelingMep.Views;

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
    public override string PluginName => nameof(RevitUnmodelingMep);
    
    /// <summary>
    /// Наименование файла конфигурации.
    /// </summary>
    /// <remarks>
    /// Используется для сохранения положения окна.
    /// </remarks>
    public override string ProjectConfigName => nameof(MainWindow);
    
    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        if(DataContext is MainViewModel viewModel) {
            if(!viewModel.AcceptViewCommand.CanExecute(null)) {
                return;
            }
        }

        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }

    private void ContentTabs_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
        if(e.Source is TabControl && DataContext is MainViewModel viewModel) {
            viewModel.RefreshAssignmentsFromConsumableTypes();
        }
    }
}
