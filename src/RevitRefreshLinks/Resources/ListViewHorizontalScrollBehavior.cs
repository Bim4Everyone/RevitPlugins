using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Microsoft.Xaml.Behaviors;

namespace RevitRefreshLinks.Resources;
internal class ListViewHorizontalScrollBehavior : Behavior<ListView> {
    protected override void OnAttached() {
        base.OnAttached();
        AssociatedObject.PreviewMouseWheel += OnPreviewMouseWheel;
    }

    protected override void OnDetaching() {
        base.OnDetaching();
        AssociatedObject.PreviewMouseWheel -= OnPreviewMouseWheel;
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e) {
        if(Keyboard.Modifiers == ModifierKeys.Shift) {
            var scrollViewer = GetScrollViewer(AssociatedObject);
            if(scrollViewer != null) {
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
                e.Handled = true;
            }
        }
    }

    private ScrollViewer GetScrollViewer(DependencyObject obj) {
        if(obj is ScrollViewer) {
            return (ScrollViewer) obj;
        }

        for(int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++) {
            var child = VisualTreeHelper.GetChild(obj, i);
            var result = GetScrollViewer(child);
            if(result != null) {
                return result;
            }
        }
        return null;
    }
}
