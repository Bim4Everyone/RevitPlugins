using System.Windows.Media;

using Color = Autodesk.Revit.DB.Color;

namespace RevitOverridingGraphicsInViews.Models;
internal class ColorHelper {
    public ColorHelper(byte R, byte G, byte B) {
        UserColor = new Color(R, G, B);
        UserBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(R, G, B));
    }

    public Color UserColor { get; set; }
    public SolidColorBrush UserBrush { get; set; }
}
