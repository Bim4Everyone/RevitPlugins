namespace RevitDocumenter.Models.RoomDimensions.Geometry;

/// <summary>
/// Отрезок на плане. Координаты во внутренних единицах Revit (футы).
/// </summary>
internal readonly struct Segment2D {
    public Segment2D(Point2D start, Point2D end) {
        Start = start;
        End = end;
    }

    public Point2D Start { get; }
    public Point2D End { get; }

    public double Length => (End - Start).Length;

    /// <summary>
    /// Единичный вектор направления от начала к концу.
    /// </summary>
    public Point2D Direction => (End - Start).Normalize();

    public Point2D Midpoint => new((Start.X + End.X) / 2, (Start.Y + End.Y) / 2);

    /// <summary>
    /// Проекция точки на направление отрезка, отсчитанная от его начала.
    /// </summary>
    public double ProjectParameter(Point2D point) {
        return (point - Start).Dot(Direction);
    }

    /// <summary>
    /// Параллельны ли отрезки с заданным допуском по синусу угла.
    /// </summary>
    public bool IsParallelTo(Segment2D other, double sinTolerance) {
        return System.Math.Abs(Direction.Cross(other.Direction)) < sinTolerance;
    }

    /// <summary>
    /// Перпендикулярны ли отрезки с заданным допуском по косинусу угла.
    /// </summary>
    public bool IsPerpendicularTo(Segment2D other, double cosTolerance) {
        return System.Math.Abs(Direction.Dot(other.Direction)) < cosTolerance;
    }
}
