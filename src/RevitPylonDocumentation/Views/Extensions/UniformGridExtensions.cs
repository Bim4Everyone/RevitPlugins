using System.Windows;
using System.Windows.Controls.Primitives;

namespace RevitPylonDocumentation.Views.Extensions;
internal static class UniformGridExtensions {
    public static readonly DependencyProperty AutoColumnsProperty =
        DependencyProperty.RegisterAttached(
            "AutoColumns",
            typeof(bool),
            typeof(UniformGridExtensions),
            new PropertyMetadata(false, OnAutoColumnsChanged));

    public static bool GetAutoColumns(UniformGrid obj) => (bool) obj.GetValue(AutoColumnsProperty);
    public static void SetAutoColumns(UniformGrid obj, bool value) => obj.SetValue(AutoColumnsProperty, value);

    private static void OnAutoColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if(d is UniformGrid uniformGrid) {
            if((bool) e.NewValue) {
                uniformGrid.SizeChanged += UniformGrid_SizeChanged;
                UpdateColumns(uniformGrid);
            } else {
                uniformGrid.SizeChanged -= UniformGrid_SizeChanged;
            }
        }
    }

    private static void UniformGrid_SizeChanged(object sender, SizeChangedEventArgs e) {
        if(sender is UniformGrid uniformGrid) {
            UpdateColumns(uniformGrid);
        }
    }

    private static void UpdateColumns(UniformGrid uniformGrid) {
        uniformGrid.Columns = uniformGrid.ActualWidth switch {
            > 1300 => 3,
            > 900 => 2,
            _ => 1
        };
    }
}
