using System.Windows;
using System.Windows.Controls;

using Microsoft.Xaml.Behaviors;

namespace RevitServerFolders.Behaviors;

public class SelectedItemTreeViewBehavior : Behavior<TreeView> {
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(
            "SelectedItem",
            typeof(object), typeof(SelectedItemTreeViewBehavior),
            new UIPropertyMetadata(null, OnSelectedItemChanged));


    public object SelectedItem {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }


    protected override void OnAttached() {
        base.OnAttached();

        AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
    }

    protected override void OnDetaching() {
        base.OnDetaching();

        if(AssociatedObject is not null) {
            AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
        }
    }

    private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
        if(e.NewValue is TreeViewItem item) {
            item.SetValue(TreeViewItem.IsSelectedProperty, true);
        }
    }

    private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
        SelectedItem = e.NewValue;
    }
}
