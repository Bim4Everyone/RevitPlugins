using System.ComponentModel;
using System.Windows;

using Microsoft.Xaml.Behaviors;

namespace RevitClashDetective.Resources;

internal class WindowClosingBehavior : Behavior<Window> {
    public IWindowClosingHandler Handler {
        get => (IWindowClosingHandler) GetValue(HandlerProperty);
        set => SetValue(HandlerProperty, value);
    }

    public static readonly DependencyProperty HandlerProperty =
        DependencyProperty.Register(
            nameof(Handler),
            typeof(IWindowClosingHandler),
            typeof(WindowClosingBehavior),
            new PropertyMetadata(null));

    protected override void OnAttached() {
        AssociatedObject.Closing += OnClosing;
    }

    protected override void OnDetaching() {
        AssociatedObject.Closing -= OnClosing;
    }

    private void OnClosing(object sender, CancelEventArgs e) {
        var handler = Handler ?? AssociatedObject.DataContext as IWindowClosingHandler;
        handler?.OnWindowClosing(e);
    }
}
