using System.Collections;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Xaml.Behaviors;

namespace RevitHideWorkset.Behaviors;
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

        SelectedItems.Clear();
        foreach(object item in AssociatedObject.SelectedItems) {
            SelectedItems.Add(item);
        }
    }
}
