using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Interfaces {
    /// <summary>
    /// Модель представления входящего задания на отверстие для КР.
    /// Использовать для моделей представления входящих заданий из АР и из ВИС в КР.
    /// </summary>
    internal interface IOpeningTaskIncomingForKrViewModel : ISelectorAndHighlighter {
        /// <summary>
        /// Id входящего задания на отверстие
        /// </summary>
        ElementId OpeningId { get; }

        /// <summary>
        /// Название файла, в котором создано задание на отверстие
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Диаметр задания на отверстие, если есть
        /// </summary>
        string Diameter { get; }

        /// <summary>
        /// Высота задания на отверстие, если есть
        /// </summary>
        string Height { get; }

        /// <summary>
        /// Ширина задания на отверстие, если есть
        /// </summary>
        string Width { get; }

        /// <summary>
        /// Статус задания на отверстие
        /// </summary>
        string Status { get; }

        /// <summary>
        /// Комментарий задания на отверстие
        /// </summary>
        string Comment { get; }
    }
}
