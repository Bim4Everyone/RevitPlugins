using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitDocumenter.Models.RoomDimensions.Geometry;

/// <summary>
/// Ломаная вдоль дуги открывания (D79), а не заполненный сектор движения двери.
/// Координаты XY в футах модели; точки получены из доступной символики активного вида.
/// </summary>
internal class DoorOpeningArcObstacle {
    public DoorOpeningArcObstacle(long doorId, IReadOnlyList<Point2D> points) {
        points.ThrowIfNull();
        if(points.Count < 2) {
            throw new ArgumentException("Для дуги открывания нужны хотя бы две точки", nameof(points));
        }
        DoorId = doorId;
        Points = points;
        Segments = Enumerable.Range(0, points.Count - 1)
            .Select(i => new Segment2D(points[i], points[i + 1])).ToList();
        Min = new Point2D(points.Min(p => p.X), points.Min(p => p.Y));
        Max = new Point2D(points.Max(p => p.X), points.Max(p => p.Y));
    }

    public long DoorId { get; }
    public IReadOnlyList<Point2D> Points { get; }
    public IReadOnlyList<Segment2D> Segments { get; }
    public Point2D Min { get; }
    public Point2D Max { get; }
}
