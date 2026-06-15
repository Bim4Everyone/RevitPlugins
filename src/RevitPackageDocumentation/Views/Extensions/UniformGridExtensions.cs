using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace RevitPackageDocumentation.Views.Extensions;
internal static class UniformGridExtensions {
    public static readonly DependencyProperty AutoColumnsProperty =
        DependencyProperty.RegisterAttached(
            "AutoColumns",
            typeof(bool),
            typeof(UniformGridExtensions),
            new PropertyMetadata(false, OnAutoColumnsChanged));

    public static readonly DependencyProperty ColumnThresholdsProperty =
        DependencyProperty.RegisterAttached(
            "ColumnThresholds",
            typeof(string),
            typeof(UniformGridExtensions),
            new PropertyMetadata("1300,900", OnThresholdsChanged));

    public static bool GetAutoColumns(UniformGrid obj) => (bool) obj.GetValue(AutoColumnsProperty);
    public static void SetAutoColumns(UniformGrid obj, bool value) => obj.SetValue(AutoColumnsProperty, value);

    public static string GetColumnThresholds(UniformGrid obj) => (string) obj.GetValue(ColumnThresholdsProperty);
    public static void SetColumnThresholds(UniformGrid obj, string value) => obj.SetValue(ColumnThresholdsProperty, value);


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

    private static void OnThresholdsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if(d is UniformGrid uniformGrid && GetAutoColumns(uniformGrid)) {
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

    //private static void UpdateColumns(UniformGrid uniformGrid) {
    //    uniformGrid.Columns = uniformGrid.ActualWidth switch {
    //        > 1300 => 3,
    //        > 900 => 2,
    //        _ => 1
    //    };
    //}

    private static void UpdateColumns(UniformGrid uniformGrid) {
        string thresholds = GetColumnThresholds(uniformGrid);
        int[] widths = thresholds.Split(',').Select(int.Parse).ToArray();

        // Пример: "1000,700,400" означает:
        // >1000 → 4 колонки
        // >700  → 3 колонки
        // >400  → 2 колонки
        // ≤400  → 1 колонка

        for(int i = 0; i < widths.Length; i++) {
            if(uniformGrid.ActualWidth > widths[i]) {
                uniformGrid.Columns = widths.Length - i + 1;
                return;
            }
        }
        uniformGrid.Columns = 1;
    }
}
