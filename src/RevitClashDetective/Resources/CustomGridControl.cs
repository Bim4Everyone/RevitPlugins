using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Grid;

namespace RevitClashDetective.Resources;
/// <summary>
/// Кастомный грид, в котором можно делать кнопки в ячейках, которые не будут менять SelectedItems, когда их больше 1
/// </summary>
internal class CustomGridControl : GridControl {
    public bool PreventSelectedItemsResetByCellButton { get; set; }

    // https://supportcenter.devexpress.com/ticket/details/t410340/disable-select-unselect-row-when-clicking-on-button-in-a-column
    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e) {
        if(PreventSelectedItemsResetByCellButton
            && SelectedItems.Count > 1
            && LayoutTreeHelper.GetVisualParents((DependencyObject) e.OriginalSource)
            .OfType<Button>()
            .FirstOrDefault() != null) {
            return;
        }
        base.OnPreviewMouseLeftButtonDown(e);
    }
}
