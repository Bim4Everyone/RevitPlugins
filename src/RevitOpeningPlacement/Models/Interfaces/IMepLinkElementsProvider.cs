using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Interfaces {
    /// <summary>
    /// Интерфейс, представляющий обертку над связанным файлом ВИС для получения из него элементов инженерных систем и входящих заданий на отверстия.
    /// <para>Использовать для обертки связей АР и КР</para>
    /// </summary>
    interface IMepLinkElementsProvider {
        /// <summary>
        /// Документ связанного файла с элементами ВИС
        /// </summary>
        Document Document { get; }

        /// <summary>
        /// Трансформация <see cref="Document">связанного файла</see> относительно активного документа ревита
        /// </summary>
        Transform DocumentTransform { get; }

        /// <summary>
        /// Возвращает коллекцию Id элементов инженерных систем из <see cref="Document">связанного файла</see>
        /// </summary>
        /// <returns></returns>
        ICollection<ElementId> GetMepElementIds();

        /// <summary>
        /// Возвращает коллекцию Id элементов заданий на отверстия из <see cref="Document">связанного файла</see>
        /// </summary>
        /// <returns></returns>
        ICollection<ElementId> GetOpeningsTaskIds();
    }
}
