using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RevitLintelsManager.Views;

/// <summary>Компактное немодальное представление снимка, принадлежащее окну менеджера.</summary>
public partial class ManagerDetailsWindow {
    /// <summary>Создаёт временное окно; ресурсами темы владеет ManagerWindow.</summary>
    public ManagerDetailsWindow() {
        InitializeComponent();
    }

    // Обрезает содержимое по форме рамки, сохраняя прозрачные углы при любой версии Windows.
    private void OnFrameSizeChanged(object sender, SizeChangedEventArgs e) {
        var frame = (Border) sender;
        double radius = frame.CornerRadius.TopLeft;
        frame.Clip = new RectangleGeometry(new Rect(e.NewSize), radius, radius);
    }

    // Закрывает информационное окно кнопкой в собственном заголовке.
    private void OnCloseClicked(object sender, RoutedEventArgs e) {
        Close();
    }

    // Сохраняет перемещение информационного окна за заголовок при отсутствии системной рамки.
    private void OnHeaderMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        if(e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 1) {
            DragMove();
        }
    }

    // Escape закрывает только информационное окно, без воздействия на сессию Revit.
    private void OnPreviewKeyDown(object sender, KeyEventArgs e) {
        if(e.Key == Key.Escape) {
            Close();
            e.Handled = true;
        }
    }
}
