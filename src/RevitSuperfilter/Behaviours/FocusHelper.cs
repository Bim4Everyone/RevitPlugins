using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RevitSuperfilter.Behaviours;

internal sealed class FocusHelper {
    public static readonly DependencyProperty IsFocusedProperty =
        DependencyProperty.RegisterAttached(
            "IsFocused",
            typeof(bool),
            typeof(FocusHelper),
            new UIPropertyMetadata(false, OnIsFocusedChanged));

    public static bool GetIsFocused(DependencyObject obj) => (bool) obj.GetValue(IsFocusedProperty);
    public static void SetIsFocused(DependencyObject obj, bool value) => obj.SetValue(IsFocusedProperty, value);

    private static void OnIsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if((bool) e.NewValue
           && d is TreeViewItem tvi) {
            tvi.Focus();

            tvi.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Loaded,
                () => {
                    var treeView = FindParent<TreeView>(tvi);
                    if(treeView == null) return;

                    var scrollViewer = FindVisualChild<ScrollViewer>(treeView);
                    if(scrollViewer == null) return;

                    try {
                        var transform = tvi.TransformToAncestor(scrollViewer);
                        var relativePoint = transform.Transform(new Point(0, 0));

                        double newOffset = scrollViewer.VerticalOffset + relativePoint.Y;
                        scrollViewer.ScrollToVerticalOffset(newOffset);
                    } catch(System.InvalidOperationException) {
                        // do nothing
                    }
                });
        }
    }

    private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject {
        for(int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++) {
            var child = VisualTreeHelper.GetChild(obj, i);
            if(child is T t) return t;
            var descendent = FindVisualChild<T>(child);
            if(descendent != null) return descendent;
        }

        return null;
    }

    private static T FindParent<T>(DependencyObject child) where T : DependencyObject {
        while(true) {
            var parentObject = VisualTreeHelper.GetParent(child);
            if(parentObject == null)
                return null;
            if(parentObject is T parent)
                return parent;
            child = parentObject;
        }
    }
}
