using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.RoomDimensions.Contours;

/// <summary>
/// Элемент контура помещения: линия или контурная точка. Хранит привязку для размера.
/// </summary>
internal abstract class ContourElement {
    /// <summary>
    /// Порядковый номер в петле контура.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Привязка для размера. null, если не найдена.
    /// </summary>
    public Reference Reference { get; set; }

    /// <summary>
    /// Каким способом найдена привязка (для журнала).
    /// </summary>
    public string ReferenceMethod { get; set; }

    /// <summary>
    /// Почему привязка не найдена (для журнала).
    /// </summary>
    public string ReferenceError { get; set; }
}
