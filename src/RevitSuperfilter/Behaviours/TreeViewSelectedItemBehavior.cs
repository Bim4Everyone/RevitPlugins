using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace RevitSuperfilter.Behaviours;

internal sealed class TreeViewSelectedItemBehavior : Behavior<TreeView> {
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(TreeViewSelectedItemBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public object SelectedItem {
        get { return GetValue(SelectedItemProperty); }
        set { SetValue(SelectedItemProperty, value); }
    }
    
    protected override void OnAttached() {
        base.OnAttached();
        AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
    }

    protected override void OnDetaching() {
        base.OnDetaching();
        if(AssociatedObject != null) {
            AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
        }
    }

    private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
        SelectedItem = e.NewValue;
    }
}
