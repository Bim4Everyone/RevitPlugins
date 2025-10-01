using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Interfaces;
/// <summary>
/// Интерфейс, предоставляющий элемент для выделения графики на виде
/// </summary>
internal interface IElementToHighlightProvider {
    /// <summary>
    /// Элемент, графику которого надо выделить на виде
    /// </summary>
    Element GetElementToHighlight();
}
