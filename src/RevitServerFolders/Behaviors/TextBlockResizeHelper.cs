using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.Xaml.Behaviors;

namespace RevitServerFolders.Behaviors;
internal class TextBlockResizeHelper : Behavior<TextBlock> {
    protected override void OnAttached() {
        base.OnAttached();
        AssociatedObject.SizeChanged += OnSizeChanged;
        AssociatedObject.LayoutUpdated += OnLayoutUpdated;
        AssociatedObject.ManipulationCompleted += OnManipulationCompleted;
    }

    protected override void OnDetaching() {
        base.OnDetaching();
        AssociatedObject.SizeChanged -= OnSizeChanged;
        AssociatedObject.LayoutUpdated -= OnLayoutUpdated;
        AssociatedObject.ManipulationCompleted -= OnManipulationCompleted;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
        UpdateTextBinding();
    }

    private void OnLayoutUpdated(object sender, System.EventArgs e) {
        UpdateTextBinding();
    }

    private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e) {
        UpdateTextBinding();
    }

    private void UpdateTextBinding() {
        var be = AssociatedObject.GetBindingExpression(TextBlock.TextProperty);
        be?.UpdateTarget();

        var beWidth = AssociatedObject.GetBindingExpression(FrameworkElement.WidthProperty);
        beWidth?.UpdateTarget();
    }
}
