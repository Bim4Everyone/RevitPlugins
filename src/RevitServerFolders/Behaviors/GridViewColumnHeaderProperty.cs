using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace RevitServerFolders.Behaviors;
/// <summary>
/// Прикрепляемое свойство для запрета изменения ширины столбца <see cref="GridView"/>.
/// У <see cref="GridViewColumnHeader"/> нет своего свойства для этого,
/// поэтому скрывается ползунок из шаблона шапки.
/// </summary>
internal static class GridViewColumnHeaderProperty {
    public static readonly DependencyProperty CanUserResizeProperty =
        DependencyProperty.RegisterAttached(
            "CanUserResize",
            typeof(bool),
            typeof(GridViewColumnHeaderProperty),
            new FrameworkPropertyMetadata(true, OnCanUserResizeChanged));

    public static bool GetCanUserResize(GridViewColumnHeader header) {
        return (bool) header.GetValue(CanUserResizeProperty);
    }

    public static void SetCanUserResize(GridViewColumnHeader header, bool value) {
        header.SetValue(CanUserResizeProperty, value);
    }

    private static void OnCanUserResizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if(d is GridViewColumnHeader header) {
            header.Loaded -= OnHeaderLoaded;
            if(e.NewValue is false) {
                header.Loaded += OnHeaderLoaded;
            }
        }
    }

    private static void OnHeaderLoaded(object sender, RoutedEventArgs e) {
        var header = (GridViewColumnHeader) sender;
        if(header.Template?.FindName("PART_HeaderGripper", header) is Thumb gripper) {
            gripper.Visibility = Visibility.Collapsed;
        }
    }
}
