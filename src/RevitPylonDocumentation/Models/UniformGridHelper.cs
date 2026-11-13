namespace RevitPylonDocumentation.Models;
internal static class UniformGridHelper {
    public static void HandleSizeChanged(object sender) {
        if(sender is not System.Windows.Controls.Primitives.UniformGrid uniformGrid)
            return;

        uniformGrid.Columns = uniformGrid.ActualWidth switch {
            > 1300 => 3,
            > 900 => 2,
            _ => 1
        };
    }
}
