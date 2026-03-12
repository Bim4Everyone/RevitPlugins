using System.Windows;
using System.Windows.Controls;

using Microsoft.Xaml.Behaviors;

namespace RevitParamsChecker.Resources;

/// <summary>
/// Скроллит ListView на SelectedItem
/// </summary>
internal class ScrollIntoViewBehavior : Behavior<ListView> {
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(ScrollIntoViewBehavior),
            new PropertyMetadata(null, OnSelectedItemChanged));

    public object SelectedItem {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var behavior = (ScrollIntoViewBehavior) d;
        if(behavior.AssociatedObject != null
           && e.NewValue != null) {
            behavior.AssociatedObject.ScrollIntoView(e.NewValue);
        }
    }
}
