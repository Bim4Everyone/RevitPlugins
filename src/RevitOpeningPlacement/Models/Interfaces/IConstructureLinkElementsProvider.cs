using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Interfaces {
    /// <summary>
    /// Интерфейс, представляющий обертку над связанным файлом ревита для получения из него элементов конструкций - стен и перекрытий.
    /// <para>Использовать для обертки связей АР и КР</para>
    /// </summary>
    internal interface IConstructureLinkElementsProvider {
        /// <summary>
        /// Документ связанного файла с конструкциями (АР или КР)
        /// </summary>
        Document Document { get; }

        /// <summary>
        /// Трансформация <see cref="Document">связанного файла</see> относительно активного документа ревита
        /// </summary>
        Transform DocumentTransform { get; }

        /// <summary>
        /// Возвращает коллекцию Id элементов категорий "Стены" и "Перекрытия" из <see cref="Document">связанного файла</see>
        /// </summary>
        /// <returns></returns>
        ICollection<ElementId> GetConstructureElementIds();

        /// <summary>
        /// Возвращает коллекцию чистовых отверстий из <see cref="Document">связанного файла</see>
        /// </summary>
        /// <returns></returns>
        ICollection<IOpeningReal> GetOpeningsReal();
    }
}
