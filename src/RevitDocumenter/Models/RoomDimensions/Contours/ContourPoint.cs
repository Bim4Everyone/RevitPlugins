using RevitDocumenter.Models.RoomDimensions.Geometry;

namespace RevitDocumenter.Models.RoomDimensions.Contours;

/// <summary>
/// Контурная точка: стык двух линий контура под углом, отличным от 90 градусов.
/// </summary>
internal class ContourPoint : ContourElement {
    public Point2D Location { get; set; }

    /// <summary>
    /// Отметка точки (футы).
    /// </summary>
    public double Z { get; set; }

    /// <summary>
    /// Угол между направлениями соседних линий, градусы.
    /// </summary>
    public double AngleDeg { get; set; }

    public ContourLine Previous { get; set; }
    public ContourLine Next { get; set; }
}
