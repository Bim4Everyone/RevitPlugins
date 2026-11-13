using Autodesk.Revit.DB;

namespace RevitPunchingRebar.Models;

/// <summary>
/// Пилон, выбранный в связанном файле.
/// </summary>
internal class LinkedPylonReference {
    /// <summary>
    /// Создает ссылку на пилон в связанном файле.
    /// </summary>
    /// <param name="linkInstanceId">Id экземпляра связи в текущем проекте.</param>
    /// <param name="pylonId">Id пилона в связанном файле.</param>
    public LinkedPylonReference(ElementId linkInstanceId, ElementId pylonId) {
        LinkInstanceId = linkInstanceId;
        PylonId = pylonId;
    }

    /// <summary>
    /// Id экземпляра связи в текущем проекте.
    /// </summary>
    public ElementId LinkInstanceId { get; }

    /// <summary>
    /// Id пилона в связанном файле.
    /// </summary>
    public ElementId PylonId { get; }
}
