using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using dosymep.SimpleServices;

namespace RevitRsnManager.Views;

/// <summary>
/// Класс главного окна плагина.
/// </summary>
public partial class MainWindow {
    private Point _dragStartPoint;
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
    public override string PluginName => nameof(RevitRsnManager);

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

    private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        _dragStartPoint = e.GetPosition(null);
    }

    private void ListBoxItem_PreviewMouseMove(object sender, MouseEventArgs e) {
        if(e.LeftButton == MouseButtonState.Pressed) {
            var currentPosition = e.GetPosition(null);
            var diff = _dragStartPoint - currentPosition;

            if(Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
               Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance) {
                if(sender is ListBoxItem item) {
                    _ = DragDrop.DoDragDrop(item, item.DataContext, DragDropEffects.Move);
                }
            }
        }
    }

    private void ListBox_Drop(object sender, DragEventArgs e) {
        if(e.Data.GetDataPresent(typeof(string))) {
            if(DataContext is RevitRsnManager.ViewModels.MainViewModel vm) {
                var servers = vm.Servers;
                if(servers == null || e.Data.GetData(typeof(string)) is not string droppedData || ((FrameworkElement) e.OriginalSource).DataContext is not string target || droppedData == target) {
                    return;
                }

                int oldIndex = servers.IndexOf(droppedData);
                int newIndex = servers.IndexOf(target);

                if(oldIndex >= 0 && newIndex >= 0) {
                    servers.Move(oldIndex, newIndex);
                }
            }
        }
    }

}
