using System.Collections;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Xaml.Behaviors;

namespace RevitRooms.Behaviors;
public class SelectedItemsBehavior : Behavior<ListBox> {
    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.Register(nameof(SelectedItems), typeof(IList), typeof(SelectedItemsBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public IList SelectedItems {
        get => (IList) GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    protected override void OnAttached() {
        base.OnAttached();
        AssociatedObject.SelectionChanged += OnSelectionChanged;
    }

    protected override void OnDetaching() {
        base.OnDetaching();
        AssociatedObject.SelectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
        if(SelectedItems == null) {
            return;
        }

        foreach(object item in e.RemovedItems) {
            if(SelectedItems.Contains(item)) {
                SelectedItems.Remove(item);
            }
        }

        foreach(object item in e.AddedItems) {
            if(!SelectedItems.Contains(item)) {
                SelectedItems.Add(item);
            }
        }
    }
}
