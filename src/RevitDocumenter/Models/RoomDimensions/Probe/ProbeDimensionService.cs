using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.Dimensions.DimensionServices;
using RevitDocumenter.Models.RoomDimensions.Contours;
using RevitDocumenter.Models.RoomDimensions.Layout;

namespace RevitDocumenter.Models.RoomDimensions.Probe;

/// <summary>
/// Ставит пробные размеры: каждая линия контура образмеривается между привязками соседних элементов.
/// Нужен, чтобы проверить, какие привязки принимает Revit (без сращивания и объединения).
/// </summary>
internal class ProbeDimensionService {
    // Допуск параллельности соседних линий по синусу угла
    private const double _angleTolerance = 1e-4;
    // Допуск сравнения значения размера с длиной линии, мм
    private const double _valueToleranceMm = 1.0;

    private readonly DimensionCreator _dimensionCreator;

    public ProbeDimensionService(DimensionCreator dimensionCreator) {
        _dimensionCreator = dimensionCreator.ThrowIfNull();
    }

    /// <param name="offset">Отступ размерной линии от линии контура в сторону помещения, футы.</param>
    /// <param name="textAllowance">Высота текста с зазором, добавляемая к отступу, когда текст обращен к контуру, футы.</param>
    public List<ProbeResult> Run(RoomContour contour, DimensionType dimensionType, double offset, double textAllowance) {
        contour.ThrowIfNull();
        dimensionType.ThrowIfNull();

        var results = new List<ProbeResult>();
        foreach(var loop in contour.Loops) {
            for(int position = 0; position < loop.Elements.Count; position++) {
                if(loop.Elements[position] is not ContourLine line) {
                    continue;
                }
                results.Add(Probe(loop, position, line, dimensionType, offset, textAllowance));
            }
        }
        return results;
    }

    private ProbeResult Probe(
        ContourLoop loop,
        int position,
        ContourLine line,
        DimensionType dimensionType,
        double offset,
        double textAllowance) {
        var previous = loop.GetPrevious(position);
        var next = loop.GetNext(position);
        var result = new ProbeResult {
            LoopIndex = loop.Index,
            LineIndex = line.Index,
            PreviousIndex = previous.Index,
            NextIndex = next.Index,
            ExpectedMm = UnitUtilsHelper.ConvertFromInternalValue(line.Segment.Length)
        };

        if(line.IsArc) {
            result.Status = "Arc";
            return result;
        }
        if(line.IsFiltered) {
            result.Status = "Filtered";
            return result;
        }
        // Соседняя линия на той же прямой: такие линии срастятся на шаге нормализации
        if(IsParallelLine(previous, line) || IsParallelLine(next, line)) {
            result.Status = "NeighborParallel";
            return result;
        }
        if(previous.Reference is null || next.Reference is null) {
            result.Status = "NeighborNoReference";
            return result;
        }

        // Текст между размерной линией и контуром прижат к стене: отодвигаем линию на высоту текста
        result.TextFacesContour = DimensionTextHelper.TextFacesContour(line.Segment.Direction, line.InwardNormal);
        double lineOffset = offset + (result.TextFacesContour ? textAllowance : 0);
        result.OffsetMm = UnitUtilsHelper.ConvertFromInternalValue(lineOffset);

        var shift = line.InwardNormal * lineOffset;
        var start = line.Segment.Start + shift;
        var end = line.Segment.End + shift;
        var dimensionLine = Line.CreateBound(new XYZ(start.X, start.Y, line.Z), new XYZ(end.X, end.Y, line.Z));

        var references = new ReferenceArray();
        references.Append(previous.Reference);
        references.Append(next.Reference);

        try {
            var dimension = _dimensionCreator.Create(dimensionLine, references, dimensionType);
            if(dimension is null) {
                result.Status = "NullDimension";
                return result;
            }
            result.DimensionId = dimension.Id;
            if(dimension.Value.HasValue) {
                result.ActualMm = UnitUtilsHelper.ConvertFromInternalValue(dimension.Value.Value);
            }
            result.Status = result.ActualMm.HasValue
                            && Math.Abs(result.ActualMm.Value - result.ExpectedMm) <= _valueToleranceMm
                ? "Ok"
                : "ValueMismatch";
        } catch(Exception ex) {
            result.Status = "Exception";
            result.Error = $"{ex.GetType().Name}: {ex.Message}";
        }
        return result;
    }

    private static bool IsParallelLine(ContourElement neighbor, ContourLine line) {
        return neighbor is ContourLine neighborLine
               && neighborLine.Segment.IsParallelTo(line.Segment, _angleTolerance);
    }
}
