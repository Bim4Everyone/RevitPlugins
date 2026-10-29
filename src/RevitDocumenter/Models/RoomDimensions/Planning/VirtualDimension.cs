using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.RoomDimensions.Geometry;

namespace RevitDocumenter.Models.RoomDimensions.Planning;

/// <summary>
/// Привязка виртуального размера: Reference и его положение вдоль направления размера.
/// </summary>
internal class DimensionAnchor {
    public DimensionAnchor(Reference reference, double coordinate, int loopIndex, int elementIndex) {
        Reference = reference;
        Coordinate = coordinate;
        LoopIndex = loopIndex;
        ElementIndex = elementIndex;
    }

    public Reference Reference { get; }

    /// <summary>
    /// Положение плоскости привязки вдоль направления размера, футы.
    /// </summary>
    public double Coordinate { get; }

    public int LoopIndex { get; }
    public int ElementIndex { get; }
}

/// <summary>
/// Виртуальный размер: хранит привязки и положение размерной линии до создания размера в модели.
/// </summary>
internal class VirtualDimension {
    public int Id { get; set; }

    /// <summary>
    /// Contour - по линии контура (тип 1), Internal - от внутреннего контура до стены (тип 3),
    /// Chain - цепочка, Overall - общий размер помещения (тип 2).
    /// </summary>
    public string Kind { get; set; } = "Contour";

    /// <summary>
    /// Петля контура, по линии которой создан размер.
    /// </summary>
    public int SourceLoopIndex { get; set; }

    /// <summary>
    /// Размер связан с внутренним контуром: размер пилона, тип 3 или цепочка с ними.
    /// </summary>
    public bool InvolvesInner { get; set; }

    /// <summary>
    /// Направление размера, приведенное к виду Revit: слева направо, вертикальные снизу вверх.
    /// </summary>
    public Point2D Direction { get; set; }

    /// <summary>
    /// Ось, по которой отсчитывается положение размерной линии.
    /// </summary>
    public Point2D Perpendicular => Direction.PerpendicularLeft();

    /// <summary>
    /// Единичный вектор от контура к размерной линии (нормаль внутрь помещения).
    /// </summary>
    public Point2D Side { get; set; }

    /// <summary>
    /// +1, если размерная линия отходит от контура в положительную сторону оси Perpendicular, иначе -1.
    /// </summary>
    public int SideSign => Side.Dot(Perpendicular) > 0 ? 1 : -1;

    /// <summary>
    /// Положение размерной линии по оси Perpendicular, футы.
    /// </summary>
    public double LinePosition { get; set; }

    /// <summary>
    /// Положение образмериваемой линии контура по оси Perpendicular, футы.
    /// Для объединенных размеров - ближайшее к размерной линии положение среди исходных.
    /// </summary>
    public double ContourPosition { get; set; }

    public double Z { get; set; }

    /// <summary>
    /// Привязки в порядке возрастания координаты.
    /// </summary>
    public List<DimensionAnchor> Anchors { get; set; } = [];

    /// <summary>
    /// Исходные линии контура: "петля:элемент".
    /// </summary>
    public List<string> Sources { get; set; } = [];

    public double Min => Anchors.Min(a => a.Coordinate);
    public double Max => Anchors.Max(a => a.Coordinate);

    /// <summary>
    /// Ожидаемые значения сегментов, футы.
    /// </summary>
    public List<double> ExpectedSegments => Anchors
        .Zip(Anchors.Skip(1), (a, b) => b.Coordinate - a.Coordinate)
        .ToList();

    public ElementId DimensionId { get; set; }
    public List<double?> ActualSegmentsMm { get; set; } = [];

    /// <summary>
    /// Ok, ValueMismatch, NullDimension, Exception.
    /// </summary>
    public string Status { get; set; }

    public string Error { get; set; }
}
