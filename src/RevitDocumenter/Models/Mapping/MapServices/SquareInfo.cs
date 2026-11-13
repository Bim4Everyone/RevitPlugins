using System.Drawing;

namespace RevitDocumenter.Models.Mapping.MapServices;
internal class SquareInfo {
    // true, если ВСЕ пиксели в квадрате белые
    public bool AllPixelsWhite { get; set; }

    // Начальная точка квадрата в координатах ВСЕГО изображения
    public Point StartPoint { get; set; }

    // Конечная точка квадрата в координатах ВСЕГО изображения
    public Point EndPoint { get; set; }
}
