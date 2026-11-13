using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.Mapping.ViewServices;
using RevitDocumenter.Models.RoomDimensions.Contours;
using RevitDocumenter.Models.RoomDimensions.Geometry;

namespace RevitDocumenter.Models.RoomDimensions.Diagnostics;

/// <summary>
/// Рисует контурную модель линиями детализации на активном виде для визуальной отладки.
/// </summary>
internal class ContourDebugDrawer {
    // Длина маркера контурной точки: диагональ вверх-вправо, мм
    private const double _pointMarkerLengthMm = 500;
    // Длина штриха нормали из середины линии в сторону помещения, мм
    private const double _normalTickLengthMm = 150;
    // Отступ линий контура и нормалей от контура в сторону помещения, чтобы они не сливались со стенами, мм
    private const double _contourOffsetMm = 50;
    private const int _contourLineWeight = 6;
    private const int _markerLineWeight = 6;

    private static readonly Color _outerColor = new(0, 0, 255);
    private static readonly Color _innerColor = new(0, 160, 0);
    private static readonly Color _pointColor = new(255, 0, 255);
    private static readonly Color _normalColor = new(255, 128, 0);
    private static readonly Color _noReferenceColor = new(255, 0, 0);
    private static readonly Color _filteredColor = new(128, 128, 128);

    private readonly AnchorLineService _lineService;

    public ContourDebugDrawer(AnchorLineService lineService) {
        _lineService = lineService.ThrowIfNull();
    }

    /// <summary>
    /// Рисует контуры. Вызывать внутри открытой транзакции.
    /// </summary>
    /// <returns>Количество созданных линий.</returns>
    public int Draw(IEnumerable<RoomContour> contours) {
        contours.ThrowIfNull();

        double pointMarkerLength = UnitUtilsHelper.ConvertToInternalValue(_pointMarkerLengthMm);
        double normalTickLength = UnitUtilsHelper.ConvertToInternalValue(_normalTickLengthMm);
        double contourOffset = UnitUtilsHelper.ConvertToInternalValue(_contourOffsetMm);
        var diagonal = new Point2D(1, 1).Normalize();

        int count = 0;
        foreach(var contour in contours) {
            foreach(var loop in contour.Loops) {
                var loopColor = loop.IsOuter ? _outerColor : _innerColor;

                foreach(var line in loop.Lines) {
                    var color = line.Reference is null ? _noReferenceColor
                        : line.IsFiltered ? _filteredColor
                        : loopColor;
                    var shift = line.InwardNormal * contourOffset;
                    count += TryDraw(
                        line.Segment.Start + shift, line.Segment.End + shift, line.Z, color, _contourLineWeight);

                    var middle = line.Segment.Midpoint + shift;
                    count += TryDraw(
                        middle, middle + line.InwardNormal * normalTickLength, line.Z, _normalColor, _markerLineWeight);
                }

                foreach(var point in loop.Points) {
                    var color = point.Reference is null ? _noReferenceColor : _pointColor;
                    count += TryDraw(
                        point.Location, point.Location + diagonal * pointMarkerLength, point.Z, color, _markerLineWeight);
                }
            }
        }
        return count;
    }

    private int TryDraw(Point2D start, Point2D end, double z, Color color, int weight) {
        var overrides = new OverrideGraphicSettings();
        overrides.SetProjectionLineColor(color);
        overrides.SetProjectionLineWeight(weight);
        try {
            _lineService.CreateLineWithOverrides(new XYZ(start.X, start.Y, z), new XYZ(end.X, end.Y, z), overrides);
            return 1;
        } catch(Exception) {
            // Слишком короткие линии Revit не создает, для отладки это допустимо
            return 0;
        }
    }
}
