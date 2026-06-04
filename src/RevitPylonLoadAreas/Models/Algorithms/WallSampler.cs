using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitPylonLoadAreas.Models.Geometry;

namespace RevitPylonLoadAreas.Models.Algorithms;

/// <summary>
/// Преобразует стены в набор точек-обструкций для Вороного:
/// равномерная выборка по осевой линии стены с заданным шагом, плюс концы.
/// </summary>
internal sealed class WallSampler {
    private readonly double _step;
    private readonly List<string> _warnings = new();

    public WallSampler(double stepFeet) {
        if(stepFeet <= 0) {
            throw new ArgumentOutOfRangeException(nameof(stepFeet), "Шаг семплирования должен быть положительным.");
        }
        _step = stepFeet;
    }

    public IReadOnlyList<string> Warnings => _warnings;

    public List<Point2D> Sample(IEnumerable<Wall> walls) {
        var result = new List<Point2D>();
        foreach(var wall in walls) {
            SampleOne(wall, result);
        }
        return result;
    }

    private void SampleOne(Wall wall, List<Point2D> sink) {
        if(wall.Location is not LocationCurve location || location.Curve is not Line line) {
            _warnings.Add(
                $"Стена [{wall.Id}] имеет криволинейную или отсутствующую осевую линию. " +
                $"В v1 поддерживаются только прямолинейные стены — стена пропущена.");
            return;
        }

        var a = line.GetEndPoint(0);
        var b = line.GetEndPoint(1);
        var start = new Point2D(a.X, a.Y);
        var end = new Point2D(b.X, b.Y);
        double length = start.DistanceTo(end);
        if(length < GeometryTolerance.Model) {
            return;
        }

        sink.Add(start);
        int intermediateCount = Math.Max(0, (int) Math.Floor(length / _step) - 1);
        if(intermediateCount > 0) {
            double effectiveStep = length / (intermediateCount + 1);
            var direction = end.Sub(start).Scale(1.0 / length);
            for(int i = 1; i <= intermediateCount; i++) {
                var p = start.Add(direction.Scale(effectiveStep * i));
                sink.Add(p);
            }
        }
        sink.Add(end);
    }
}
