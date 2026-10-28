using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace RevitPackageDocumentation.Views.Extensions;

/// <summary>
/// Помощник контекстных меню карточек: меню карточки должно открываться только на свободном месте,
/// а не на элементах управления внутри нее
/// </summary>
internal static class ContextMenuHelper {
    /// <summary>
    /// Проверяет, пришелся ли щелчок на элемент управления внутри карточки: поле ввода, выпадающий список,
    /// кнопку, флажок, переключатель, ползунок или полосу прокрутки
    /// </summary>
    /// <param name="originalSource">Элемент, на котором был щелчок (ContextMenuEventArgs.OriginalSource)</param>
    /// <param name="container">Карточка, на которой задано меню. Поиск идет вверх по дереву до нее</param>
    public static bool IsOverInputControl(object originalSource, DependencyObject container) {
        var current = originalSource as DependencyObject;
        while(current != null && current != container) {
            if(current is TextBoxBase or PasswordBox or ComboBox or ButtonBase or Slider or ScrollBar) {
                return true;
            }
            // Текстовые элементы (Run и т.п.) не являются Visual - для них поднимаемся по логическому дереву
            current = current is Visual
                ? VisualTreeHelper.GetParent(current)
                : LogicalTreeHelper.GetParent(current);
        }
        return false;
    }
}
