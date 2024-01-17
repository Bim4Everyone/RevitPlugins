using System.ComponentModel;

namespace RevitFinishingWalls.Models.Enums {
    internal enum WallHeightMode {
        [Description("Задать вручную")]
        ManualHeight,
        [Description("По помещениям")]
        HeightByRoom
    }
}
