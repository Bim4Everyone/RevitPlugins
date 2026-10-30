using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using RevitStairRailing.ViewModels;

using dosymep.SimpleServices;

namespace RevitStairRailing.Views;

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
    public override string PluginName => nameof(RevitStairRailing);
    
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

    /// <summary>
    /// Поле, в котором начат ввод, выделяет свою часть схемы: часть схемы — в Tag поля
    /// (у выпадающих списков фокус получает вложенный элемент, поэтому Tag ищется вверх по дереву).
    /// </summary>
    private void Content_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
        string? field = null;
        for(DependencyObject? element = e.NewFocus as DependencyObject; element != null && element != sender;
            element = VisualTreeHelper.GetParent(element) ?? LogicalTreeHelper.GetParent(element)) {
            if(element is FrameworkElement { Tag: string tag }) {
                field = tag;
                break;
            }
        }

        ((MainViewModel) DataContext).ActiveField = field;
    }
}
