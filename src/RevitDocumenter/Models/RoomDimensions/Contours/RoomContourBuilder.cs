using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using RevitDocumenter.Models.RoomDimensions.Geometry;
using RevitDocumenter.Models.RoomDimensions.Settings;

namespace RevitDocumenter.Models.RoomDimensions.Contours;

/// <summary>
/// Строит контурную модель помещения: петли, источники линий, нормали внутрь и контурные точки.
/// </summary>
internal class RoomContourBuilder {
    // Отрезки короче этого значения (футы) считаются вырожденными и отбрасываются
    private const double _minSegmentLength = 1e-6;
    // Допуск по синусу/косинусу угла: около 0.006 градуса
    private const double _angleTolerance = 1e-4;

    private readonly ContourNormalizer _normalizer;

    public RoomContourBuilder(ContourNormalizer normalizer) {
        _normalizer = normalizer.ThrowIfNull();
    }

    public RoomContour Build(Room room, RoomTypeSettings roomType) {
        room.ThrowIfNull();
        roomType.ThrowIfNull();

        var contour = new RoomContour(room, roomType);
        var location = ParseBoundaryLocation(roomType.BoundaryLocation);
        contour.BoundaryLocation = location.ToString();

        var options = new SpatialElementBoundaryOptions { SpatialElementBoundaryLocation = location };
        IList<IList<BoundarySegment>> boundary;
        try {
            boundary = room.GetBoundarySegments(options);
        } catch(Exception ex) {
            contour.Error = $"GetBoundarySegments: {ex.Message}";
            return contour;
        }
        if(boundary is null || boundary.Count == 0) {
            contour.Error = "Помещение не имеет контура";
            return contour;
        }

        var lineLoops = boundary
            .Select(CreateLines)
            .Where(lines => lines.Count >= 3)
            .ToList();
        if(lineLoops.Count == 0) {
            contour.Error = "Все петли контура вырожденные";
            return contour;
        }

        // Внешний контур - петля с наибольшей площадью
        var areas = lineLoops.Select(GetSignedArea).ToList();
        int outerIndex = areas
            .Select((area, index) => (area, index))
            .OrderByDescending(x => Math.Abs(x.area))
            .First().index;

        for(int i = 0; i < lineLoops.Count; i++) {
            bool isOuter = i == outerIndex;
            // Помещение лежит внутри внешней петли и снаружи внутренних
            bool roomOnLeft = isOuter ? areas[i] > 0 : areas[i] < 0;
            foreach(var line in lineLoops[i]) {
                var left = line.Segment.Direction.PerpendicularLeft();
                line.InwardNormal = roomOnLeft ? left : -left;
            }

            var loop = new ContourLoop {
                Index = i,
                IsOuter = isOuter,
                SignedArea = areas[i],
                RawLineCount = lineLoops[i].Count
            };

            // Нормализация до расстановки контурных точек: точки ставятся на стыках итоговых линий
            for(int k = 0; k < lineLoops[i].Count; k++) {
                lineLoops[i][k].RawIndices = [k];
            }
            var normalized = _normalizer.Normalize(
                lineLoops[i],
                UnitUtilsHelper.ConvertToInternalValue(roomType.MergeOffsetMm),
                UnitUtilsHelper.ConvertToInternalValue(roomType.MinLineLengthMm),
                loop.NormalizationLog);
            loop.Elements = InsertContourPoints(normalized);
            for(int j = 0; j < loop.Elements.Count; j++) {
                loop.Elements[j].Index = j;
            }
            contour.Loops.Add(loop);
        }

        // Внешний контур первым, затем внутренние
        contour.Loops.Sort((a, b) => b.IsOuter.CompareTo(a.IsOuter));
        return contour;
    }

    private static SpatialElementBoundaryLocation ParseBoundaryLocation(string value) {
        return Enum.TryParse(value, true, out SpatialElementBoundaryLocation location)
            ? location
            : SpatialElementBoundaryLocation.Finish;
    }

    private static List<ContourLine> CreateLines(IList<BoundarySegment> segments) {
        var lines = new List<ContourLine>();
        foreach(var segment in segments) {
            var curve = segment.GetCurve();
            var start = curve.GetEndPoint(0);
            var end = curve.GetEndPoint(1);
            var segment2D = new Segment2D(new Point2D(start.X, start.Y), new Point2D(end.X, end.Y));
            if(segment2D.Length < _minSegmentLength) {
                continue;
            }
            lines.Add(new ContourLine {
                Segment = segment2D,
                Z = start.Z,
                SourceId = segment.ElementId,
                LinkSourceId = segment.LinkElementId,
                IsArc = curve is not Line
            });
        }
        return lines;
    }

    private static double GetSignedArea(List<ContourLine> lines) {
        double sum = 0;
        foreach(var line in lines) {
            var a = line.Segment.Start;
            var b = line.Segment.End;
            sum += a.X * b.Y - b.X * a.Y;
        }
        return sum / 2;
    }

    /// <summary>
    /// Вставляет контурную точку между линиями, которые стыкуются не под прямым углом.
    /// </summary>
    private static List<ContourElement> InsertContourPoints(List<ContourLine> lines) {
        var elements = new List<ContourElement>();
        for(int i = 0; i < lines.Count; i++) {
            var line = lines[i];
            var next = lines[(i + 1) % lines.Count];
            elements.Add(line);

            bool isPerpendicular = line.Segment.IsPerpendicularTo(next.Segment, _angleTolerance);
            bool isParallel = line.Segment.IsParallelTo(next.Segment, _angleTolerance);
            if(isPerpendicular || isParallel) {
                continue;
            }

            double cos = Math.Max(-1, Math.Min(1, line.Segment.Direction.Dot(next.Segment.Direction)));
            elements.Add(new ContourPoint {
                Location = line.Segment.End,
                Z = line.Z,
                AngleDeg = Math.Acos(cos) * 180 / Math.PI,
                Previous = line,
                Next = next
            });
        }
        return elements;
    }
}
