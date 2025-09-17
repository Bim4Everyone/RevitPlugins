using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace RevitServerFolders.Behaviors;

public static class ExpandedTreeViewItemsBehavior {
    public static readonly DependencyProperty ExpandedCommandProperty =
        DependencyProperty.RegisterAttached(
            "ExpandedCommand",
            typeof(ICommand),
            typeof(ExpandedTreeViewItemsBehavior),
            new PropertyMetadata(null, OnExpandedCommandChanged));

    public static readonly DependencyProperty ExpandedCommandParameterProperty =
        DependencyProperty.RegisterAttached(
            "ExpandedCommandParameter",
            typeof(object),
            typeof(ExpandedTreeViewItemsBehavior),
            new PropertyMetadata(null));


    public static void SetExpandedCommand(DependencyObject element, ICommand value) {
        element.SetValue(ExpandedCommandProperty, value);
    }

    public static ICommand GetExpandedCommand(DependencyObject element) {
        return (ICommand) element.GetValue(ExpandedCommandProperty);
    }

    public static void SetExpandedCommandParameter(DependencyObject element, object value) {
        element.SetValue(ExpandedCommandParameterProperty, value);
    }

    public static object GetExpandedCommandParameter(DependencyObject element) {
        return element.GetValue(ExpandedCommandParameterProperty);
    }

    private static void OnExpandedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if(d is TreeViewItem item) {
            item.Expanded -= OnExpanded;

            if(e.NewValue != null) {
                item.Expanded += OnExpanded;
            }
        }
    }

    private static void OnExpanded(object sender, RoutedEventArgs e) {
        if(sender is TreeViewItem item) {
            var command = GetExpandedCommand(item);
            object parameter = GetExpandedCommandParameter(item) ?? item.DataContext;

            if(command?.CanExecute(parameter) == true) {
                command.Execute(parameter);
            }
        }
    }
}
