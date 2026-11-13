using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.ViewModels.Navigator;

/// <summary>
/// Модель представления входящего задания на отверстие для АР.
/// Использовать для моделей представления входящих заданий из ВИС в АР.
/// </summary>
internal interface IOpeningMepTaskIncomingToArViewModel : ISelectorAndHighlighter {
    /// <summary>
    /// Название связанного файла-источника задания на отверстие
    /// </summary>
    string FileName { get; }

    /// <summary>
    /// Id экземпляра семейства задания на отверстие
    /// </summary>
    ElementId OpeningId { get; }

    /// <summary>
    /// Дата создания отверстия
    /// </summary>
    string Date { get; }

    /// <summary>
    /// Название инженерной системы, для элемента которой создано задание на отверстие
    /// </summary>
    string MepSystem { get; }

    /// <summary>
    /// Описание задания на отверстие
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Отметка центра задания на отверстие
    /// </summary>
    string CenterOffset { get; }

    /// <summary>
    /// Отметка низа задания на отверстие
    /// </summary>
    string BottomOffset { get; }

    /// <summary>
    /// Диаметр задания на отверстие, если есть
    /// </summary>
    string Diameter { get; }

    /// <summary>
    /// Ширина задания на отверстие, если есть
    /// </summary>
    string Width { get; }

    /// <summary>
    /// Высота задания на отверстие, если есть
    /// </summary>
    string Height { get; }

    /// <summary>
    /// Толщина задания на отверстие
    /// </summary>
    string Thickness { get; }

    /// <summary>
    /// Статус задания на отверстие
    /// </summary>
    string Status { get; }

    /// <summary>
    /// Расположение отверстия - в перекрытии/в стене
    /// </summary>
    string FamilyShortName { get; }

    /// <summary>
    /// Комментарий экземпляра семейства задания на отверстие
    /// </summary>
    string Comment { get; }

    /// <summary>
    /// Имя пользователя, создавшего задание на отверстие
    /// </summary>
    string Username { get; }

    /// <summary>
    /// Хост задания на отверстие из активного файла АР
    /// </summary>
    IOpeningHost Host { get; }
}
