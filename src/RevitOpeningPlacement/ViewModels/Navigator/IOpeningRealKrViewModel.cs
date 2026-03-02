using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.ViewModels.Navigator;

/// <summary>
/// Модель представления чистового отверстия КР в файле КР.
/// Использовать для навигатора КР.
/// </summary>
internal interface IOpeningRealKrViewModel : ISelectorAndHighlighter {
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
    /// Статус чистового отверстия
    /// </summary>
    string Status { get; }

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

    /// <summary>
    /// Основа чистового отверстия
    /// </summary>
    IOpeningKrHost Host { get; }
}
