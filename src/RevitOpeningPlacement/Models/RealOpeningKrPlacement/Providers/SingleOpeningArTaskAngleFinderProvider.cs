using System;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.Models.RealOpeningKrPlacement.AngleFinders;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.Providers;
/// <summary>
/// Класс, предоставляющий интерфейс для определения угла поворота размещаемого отверстия КР
/// </summary>
internal class SingleOpeningArTaskAngleFinderProvider {
    private readonly IOpeningTaskIncoming _incomingTask;


    /// <summary>
    /// Конструктор класса, предоставляющего интерфейс для определения угла поворота размещаемого отверстия КР
    /// </summary>
    /// <param name="incomingTask">Входящее задание на отверстие от АР</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public SingleOpeningArTaskAngleFinderProvider(IOpeningTaskIncoming incomingTask) {
        _incomingTask = incomingTask ?? throw new ArgumentNullException(nameof(incomingTask));
    }


    public IAngleFinder GetAngleFinder() {
        return _incomingTask.OpeningType switch {
            OpeningType.WallRound or OpeningType.WallRectangle or OpeningType.FloorRound => new ZeroAngleFinder(),
            OpeningType.FloorRectangle => new SingleRectangleOpeningArTaskInFloorAngleFinder(_incomingTask),
            _ => throw new ArgumentException(nameof(_incomingTask.OpeningType)),
        };
    }
}
