using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.RoomDimensions.Geometry;

namespace RevitDocumenter.Models.RoomDimensions.Contours;

/// <summary>
/// Линия контура помещения и элемент модели, который ее образует.
/// </summary>
internal class ContourLine : ContourElement {
    public Segment2D Segment { get; set; }

    /// <summary>
    /// Отметка линии контура (футы).
    /// </summary>
    public double Z { get; set; }

    /// <summary>
    /// Единичная нормаль в сторону помещения.
    /// </summary>
    public Point2D InwardNormal { get; set; }

    /// <summary>
    /// Элемент, образующий линию контура.
    /// </summary>
    public ElementId SourceId { get; set; }

    /// <summary>
    /// Элемент связанной модели, если линия образована им.
    /// </summary>
    public ElementId LinkSourceId { get; set; }

    /// <summary>
    /// Элемент, подобранный по соседним линиям, если Revit не указал источник линии.
    /// </summary>
    public ElementId InferredSourceId { get; set; }

    /// <summary>
    /// Линия контура является дугой. В MVP дуги не образмериваются.
    /// </summary>
    public bool IsArc { get; set; }

    /// <summary>
    /// Номера исходных линий контура (до нормализации), из которых получена эта линия.
    /// </summary>
    public List<int> RawIndices { get; set; } = [];

    /// <summary>
    /// Линия короче минимальной длины: сама не образмеривается, но служит привязкой для соседей.
    /// </summary>
    public bool IsFiltered { get; set; }

    /// <summary>
    /// Исходные линии, из которых срощена эта линия. null - линия не сращивалась.
    /// </summary>
    public List<ContourLine> MergedParts { get; set; }
}
