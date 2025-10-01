using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.RealOpeningsGeometryValueGetters;
/// <summary>
/// Класс, предоставляющий методы округления для габаритов чистовых отверстий АР/КР
/// </summary>
internal abstract class RealOpeningSizeValueGetter : RoundValueGetter {
    /// <summary>
    /// Значение округления высоты в мм
    /// </summary>
    private protected readonly int _heightRound;

    /// <summary>
    /// Значение округления ширины в мм
    /// </summary>
    private protected readonly int _widthRound;

    /// <summary>
    /// Значение округления диаметра в мм
    /// </summary>
    private protected readonly int _diameterRound;


    protected RealOpeningSizeValueGetter(int heightRounding, int widthRounding, int diameterRounding) {
        _heightRound = heightRounding;
        _widthRound = widthRounding;
        _diameterRound = diameterRounding;
    }


    private protected BoundingBoxXYZ GetUnitedBox(ICollection<IOpeningTaskIncoming> incomingTasks) {
        return incomingTasks.Select(task => task.GetTransformedBBoxXYZ()).ToList().CreateUnitedBoundingBox();
    }

    /// <summary>
    /// Возвращает высоту задания на отверстие независимо от его формы
    /// </summary>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    private protected double GetOpeningHeight(IOpeningTaskIncoming incomingTask) {
        return incomingTask is null
            ? throw new ArgumentNullException(nameof(incomingTask))
            : incomingTask.Height > 0 ? incomingTask.Height : incomingTask.Diameter;
    }

    /// <summary>
    /// Возвращает ширину задания на отверстие независимо от его формы
    /// </summary>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    private protected double GetOpeningWidth(IOpeningTaskIncoming incomingTask) {
        return incomingTask is null
            ? throw new ArgumentNullException(nameof(incomingTask))
            : incomingTask.Width > 0 ? incomingTask.Width : incomingTask.Diameter;
    }

    /// <summary>
    /// Возвращает диаметр задания на отверстие независимо от его формы
    /// </summary>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    private protected double GetOpeningDiameter(IOpeningTaskIncoming incomingTask) {
        return incomingTask is null
            ? throw new ArgumentNullException(nameof(incomingTask))
            : incomingTask.Diameter > 0 ? incomingTask.Diameter : incomingTask.Height;
    }
}
