using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.Dimensions.DimensionServices;
using RevitDocumenter.Models.RoomDimensions.Geometry;

namespace RevitDocumenter.Models.RoomDimensions.Planning;

/// <summary>
/// Создает размеры в модели по виртуальным размерам и сверяет значения сегментов с ожидаемыми.
/// </summary>
internal class VirtualDimensionBuilder {
    // Допуск сравнения значения сегмента с ожидаемым, мм
    private const double _valueToleranceMm = 1.0;

    private readonly DimensionCreator _dimensionCreator;

    public VirtualDimensionBuilder(DimensionCreator dimensionCreator) {
        _dimensionCreator = dimensionCreator.ThrowIfNull();
    }

    public void Create(VirtualDimension dimension, DimensionType dimensionType) {
        dimension.ThrowIfNull();
        dimensionType.ThrowIfNull();

        Point2D PointAt(double coordinate) =>
            dimension.Direction * coordinate + dimension.Perpendicular * dimension.LinePosition;

        var start = PointAt(dimension.Min);
        var end = PointAt(dimension.Max);
        var line = Line.CreateBound(new XYZ(start.X, start.Y, dimension.Z), new XYZ(end.X, end.Y, dimension.Z));

        var references = new ReferenceArray();
        foreach(var anchor in dimension.Anchors) {
            references.Append(anchor.Reference);
        }

        try {
            var created = _dimensionCreator.Create(line, references, dimensionType);
            if(created is null) {
                dimension.Status = "NullDimension";
                return;
            }
            dimension.DimensionId = created.Id;
            dimension.ActualSegmentsMm = GetSegmentValues(created)
                .Select(v => v.HasValue ? UnitUtilsHelper.ConvertFromInternalValue(v.Value) : (double?) null)
                .ToList();

            var expected = dimension.ExpectedSegments
                .Select(UnitUtilsHelper.ConvertFromInternalValue)
                .ToList();
            bool matches = expected.Count == dimension.ActualSegmentsMm.Count
                           && expected.Zip(dimension.ActualSegmentsMm, (e, a) => a.HasValue
                                                                                && Math.Abs(a.Value - e) <= _valueToleranceMm)
                               .All(ok => ok);
            dimension.Status = matches ? "Ok" : "ValueMismatch";
        } catch(Exception ex) {
            dimension.Status = "Exception";
            dimension.Error = $"{ex.GetType().Name}: {ex.Message}";
        }
    }

    private static List<double?> GetSegmentValues(Dimension dimension) {
        if(dimension.NumberOfSegments <= 1) {
            return [dimension.Value];
        }
        var values = new List<double?>();
        foreach(DimensionSegment segment in dimension.Segments) {
            values.Add(segment.Value);
        }
        return values;
    }
}
