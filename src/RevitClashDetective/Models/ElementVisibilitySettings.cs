
using System.Windows.Media;

namespace RevitClashDetective.Models;
internal class ElementVisibilitySettings {
    public Color Color { get; set; } = Color.FromRgb(255, 255, 0);

    public int Transparency { get; set; } = 40;
}
