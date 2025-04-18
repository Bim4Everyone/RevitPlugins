using Autodesk.Revit.DB;

using RevitCreateViewSheet.Services;

namespace RevitCreateViewSheet.Models {
    internal interface IEntity {
        /// <summary>
        /// Сервис для сохранения изменений над объектом
        /// </summary>
        IEntitySaver Saver { get; }

        /// <summary>
        /// True, если элемент существует в модели Revit, иначе False
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// Пытается получить Id существующего элемента из модели Revit.
        /// </summary>
        /// <param name="id">Id существующего элемента Revit</param>
        /// <returns>True, если элемент существует в Revit, иначе False</returns>
        bool TryGetExistId(out ElementId id);
    }
}
