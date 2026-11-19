using System;
using System.Collections.Generic;
using System.Linq;

using RevitDocumenter.Models.RoomDimensions.Geometry;

namespace RevitDocumenter.Models.RoomDimensions.Contours;

/// <summary>
/// Восстанавливает полосу прямой исключаемой стены по обеим граням, включая узлы.
/// Концы ограничиваются примыкающими сохранёнными границами, а не общим габаритом комнат.
/// </summary>
internal class IgnoredWallStripBuilder {
    private const double _tolerance = 1e-7;
    private const double _angleTolerance = 1e-6;

    public List<Point2D> Build(List<ContourLine> faces, List<ContourLine> retained, out double width) {
        if(faces.Count == 0 || faces.Any(l => l.IsArc)) {
            throw new InvalidOperationException("Маячковая стена: нет прямолинейных граней");
        }
        var direction = faces.OrderByDescending(l => l.Segment.Length).First().Segment.Direction;
        if(direction.X < -_angleTolerance || (Math.Abs(direction.X) < _angleTolerance && direction.Y < 0)) {
            direction = -direction;
        }
        var normal = direction.PerpendicularLeft();
        if(faces.Any(l => Math.Abs(l.Segment.Direction.Cross(direction)) > _angleTolerance)) {
            throw new InvalidOperationException("Маячковая стена: непараллельные грани требуют отдельной обработки");
        }
        var points = faces.SelectMany(l => new[] { l.Segment.Start, l.Segment.End }).ToList();
        double lo = points.Min(p => p.Dot(direction));
        double hi = points.Max(p => p.Dot(direction));
        double sideLo = points.Min(p => p.Dot(normal));
        double sideHi = points.Max(p => p.Dot(normal));
        width = sideHi - sideLo;
        if(width <= _tolerance || faces.Any(l => {
               double coordinate = l.Segment.Midpoint.Dot(normal);
               return Math.Abs(coordinate - sideLo) > _tolerance && Math.Abs(coordinate - sideHi) > _tolerance;
           })) {
            throw new InvalidOperationException("Маячковая стена: не удалось определить две плоскости граней");
        }

        double originalLo = lo;
        double originalHi = hi;
        foreach(var line in retained.Where(l => !l.IsArc)) {
            // Сохранённая грань может завершать полосу, только если достигает её по ширине.
            if(Math.Abs(line.Segment.Direction.Dot(direction)) > _angleTolerance) {
                continue;
            }
            double a = line.Segment.Start.Dot(normal);
            double b = line.Segment.End.Dot(normal);
            if(Math.Max(a, b) < sideLo - _tolerance || Math.Min(a, b) > sideHi + _tolerance) {
                continue;
            }
            double cap = line.Segment.Start.Dot(direction);
            double inward = line.InwardNormal.Dot(direction);
            if(Math.Abs(cap - originalLo) <= width + _tolerance && inward > 0.999) {
                lo = Math.Max(lo, cap);
            }
            if(Math.Abs(cap - originalHi) <= width + _tolerance && inward < -0.999) {
                hi = Math.Min(hi, cap);
            }
        }
        if(hi - lo <= _tolerance) {
            throw new InvalidOperationException("Маячковая стена: сохранённые границы перекрывают всю полосу");
        }

        Point2D At(double along, double across) => direction * along + normal * across;
        return [At(lo, sideLo), At(hi, sideLo), At(hi, sideHi), At(lo, sideHi)];
    }
}
