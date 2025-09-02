using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using Microsoft.Xaml.Behaviors;

namespace RevitRefreshLinks.Resources;
internal class SelectorDoubleClickBehavior : Behavior<Selector> {
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(SelectorDoubleClickBehavior));

    public ICommand Command {
        get => (ICommand) GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override void OnAttached() {
        base.OnAttached();
        AssociatedObject.MouseDoubleClick += OnMouseDoubleClick;
    }

    protected override void OnDetaching() {
        base.OnDetaching();
        AssociatedObject.MouseDoubleClick -= OnMouseDoubleClick;
    }

    private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
        if(sender is Selector && e.OriginalSource is FrameworkElement originalSource) {
            object clickedItem = originalSource.DataContext;
            try {
                if(clickedItem != null && Command != null && Command.CanExecute(clickedItem)) {
                    Command.Execute(clickedItem);
                }
            } catch(InvalidCastException) {
                // pass
            }
        }
    }
}
