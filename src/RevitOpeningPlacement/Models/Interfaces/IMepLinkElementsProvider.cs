using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Interfaces;
/// <summary>
/// Интерфейс, представляющий обертку над связанным файлом ВИС для получения из него элементов инженерных систем и входящих заданий на отверстия.
/// <para>Использовать для обертки связей АР и КР</para>
/// </summary>
internal interface IMepLinkElementsProvider : ILinkElementsProvider {
    /// <summary>
    /// Возвращает коллекцию Id элементов инженерных систем из <see cref="Document">связанного файла</see>
    /// </summary>
    ICollection<ElementId> GetMepElementIds();

    /// <summary>
    /// Возвращает коллекцию Id элементов заданий на отверстия из <see cref="Document">связанного файла</see>
    /// </summary>
    ICollection<ElementId> GetOpeningsTaskIds();
}
