using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.RoomDimensions.Probe;

/// <summary>
/// Результат одного пробного размера для линии контура.
/// </summary>
internal class ProbeResult {
    public int LoopIndex { get; set; }

    /// <summary>
    /// Номер образмериваемой линии в петле.
    /// </summary>
    public int LineIndex { get; set; }

    /// <summary>
    /// Номера соседних элементов, к которым привязан размер.
    /// </summary>
    public int PreviousIndex { get; set; }
    public int NextIndex { get; set; }

    /// <summary>
    /// Ожидаемое значение: длина линии контура, мм.
    /// </summary>
    public double ExpectedMm { get; set; }

    /// <summary>
    /// Значение построенного размера, мм.
    /// </summary>
    public double? ActualMm { get; set; }

    /// <summary>
    /// Фактический отступ размерной линии от линии контура, мм.
    /// </summary>
    public double OffsetMm { get; set; }

    /// <summary>
    /// Текст размера обращен к контуру, поэтому к отступу добавлена высота текста.
    /// </summary>
    public bool TextFacesContour { get; set; }

    public ElementId DimensionId { get; set; }

    /// <summary>
    /// Ok, ValueMismatch, Filtered, NeighborParallel, NeighborNoReference, Arc, NullDimension, Exception.
    /// </summary>
    public string Status { get; set; }

    public string Error { get; set; }
}
