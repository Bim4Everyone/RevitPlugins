using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Interfaces;
/// <summary>
/// Хост задания на отверстие или хост чистового отверстия
/// </summary>
internal interface IOpeningHost {
    /// <summary>
    /// Название хоста
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Id хоста
    /// </summary>
    ElementId Id { get; }
}
