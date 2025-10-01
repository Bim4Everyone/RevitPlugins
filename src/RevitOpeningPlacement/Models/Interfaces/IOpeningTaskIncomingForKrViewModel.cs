using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Interfaces;
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
    /// Дата создания отверстия
    /// </summary>
    string Date { get; }

    /// <summary>
    /// Комментарий задания на отверстие
    /// </summary>
    string Comment { get; }

    /// <summary>
    /// Толщина задания на отверстие
    /// </summary>
    string Thickness { get; }

    /// <summary>
    /// Отметка центра задания на отверстие
    /// </summary>
    string CenterOffset { get; }

    /// <summary>
    /// Отметка низа задания на отверстие
    /// </summary>
    string BottomOffset { get; }

    /// <summary>
    /// Название инженерной системы, для элемента которой создано задание на отверстие
    /// </summary>
    string MepSystem { get; }

    /// <summary>
    /// Имя пользователя, создавшего задание на отверстие
    /// </summary>
    string Username { get; }

    /// <summary>
    /// Расположение отверстия - в перекрытии/в стене
    /// </summary>
    string FamilyShortName { get; }

    /// <summary>
    /// Описание задания на отверстие
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Хост задания на отверстие из активного файла
    /// </summary>
    IOpeningKrHost Host { get; }
}
