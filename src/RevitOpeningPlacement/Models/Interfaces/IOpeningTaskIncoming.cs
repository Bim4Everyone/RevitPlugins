using Autodesk.Revit.DB;

using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.Models.Interfaces;
/// <summary>
/// Интерфейс для входящих заданий на отверстия
/// </summary>
internal interface IOpeningTaskIncoming : ISolidProvider, IFamilyInstanceProvider {
    /// <summary>
    /// Ширина задания на отверстие в единицах Revit
    /// </summary>
    double Width { get; }

    /// <summary>
    /// Высота задания на отверстие в единицах Revit
    /// </summary>
    double Height { get; }

    /// <summary>
    /// Диаметр задания на отверстие в единицах Revit
    /// </summary>
    double Diameter { get; }

    /// <summary>
    /// Координата точки размещения задания на отверстие в координатах активного файла-получателя заданий
    /// </summary>
    XYZ Location { get; }

    /// <summary>
    /// Тип задания на отверстие
    /// </summary>
    OpeningType OpeningType { get; }

    /// <summary>
    /// Id экземпляра семейства задания на отверстие
    /// </summary>
    ElementId Id { get; }

    /// <summary>
    /// Путь к файлу, в котором создано задание на отверстие
    /// </summary>
    string FileName { get; }

    /// <summary>
    /// Угол поворота задания на отверстие в радианах в координатах активного файла, 
    /// в который подгружена связь с заданием на отверстие
    /// </summary>
    double Rotation { get; }

    /// <summary>
    /// Статус входящего задания на отверстие
    /// </summary>
    OpeningTaskIncomingStatus Status { get; set; }

    /// <summary>
    /// Хост входящего задания на отверстие из активного документа - получателя заданий
    /// </summary>
    Element Host { get; set; }
}
