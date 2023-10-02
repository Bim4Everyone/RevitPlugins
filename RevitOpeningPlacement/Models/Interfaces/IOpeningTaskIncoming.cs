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
    }
}
