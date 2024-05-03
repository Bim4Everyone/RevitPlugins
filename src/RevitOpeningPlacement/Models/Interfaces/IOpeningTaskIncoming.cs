using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Interfaces {
    /// <summary>
    /// Интерфейс для входящих заданий на отверстия
    /// </summary>
    internal interface IOpeningTaskIncoming : ISolidProvider {
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
    }
}
