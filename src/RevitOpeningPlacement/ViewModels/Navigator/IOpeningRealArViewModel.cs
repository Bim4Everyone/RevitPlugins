using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.ViewModels.Navigator;

/// <summary>
/// Модель представления чистового отверстия АР в файле АР.
/// Использовать для навигатора АР.
/// </summary>
internal interface IOpeningRealArViewModel : ISelectorAndHighlighter {
    /// <summary>
    /// Статус чистового отверстия
    /// </summary>
    string Status { get; }

    /// <summary>
    /// Id экземпляра семейства чистового на отверстия
    /// </summary>
    ElementId OpeningId { get; }

    /// <summary>
    /// Диаметр
    /// </summary>
    string Diameter { get; }

    /// <summary>
    /// Ширина
    /// </summary>
    string Width { get; }

    /// <summary>
    /// Высота
    /// </summary>
    string Height { get; }

    /// <summary>
    /// Комментарий
    /// </summary>
    string Comment { get; }

    /// <summary>
    /// Название уровня
    /// </summary>
    string LevelName { get; }

    /// <summary>
    /// Информация о задании, по которому было создано данное чистовое отверстие
    /// </summary>
    string TaskInfo { get; }

    /// <summary>
    /// Название семейства
    /// </summary>
    string FamilyName { get; }
}
