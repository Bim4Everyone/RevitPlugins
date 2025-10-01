using System;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.Models.RealOpeningArPlacement.AngleFinders;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.Providers;
/// <summary>
/// Класс, предоставляющий <see cref="IAngleFinder"/> для чистового отверстия для размещения по одному заданию на отверстие
/// </summary>
internal class SingleOpeningTaskAngleFinderProvider {
    private readonly OpeningMepTaskIncoming _openingMepTaskIncoming;


    /// <summary>
    /// Конструктор класса, предоставляющего <see cref="IAngleFinder"/>
    /// </summary>
    /// <param name="incomingTask">Входящее задание на отверстие</param> для чистового отверстия для размещения по одному заданию на отверстие
    /// <exception cref="ArgumentNullException"/>
    public SingleOpeningTaskAngleFinderProvider(OpeningMepTaskIncoming incomingTask) {
        if(incomingTask is null) { throw new ArgumentNullException(nameof(incomingTask)); }

        _openingMepTaskIncoming = incomingTask;
    }


    /// <exception cref="ArgumentException">Исключение, если тип проема задания на отверстие не поддерживается</exception>
    public IAngleFinder GetAngleFinder() {
        return _openingMepTaskIncoming.OpeningType switch {
            OpeningType.WallRound or OpeningType.WallRectangle or OpeningType.FloorRound => new ZeroAngleFinder(),
            OpeningType.FloorRectangle => new SingleRectangleOpeningTaskInFloorAngleFinder(_openingMepTaskIncoming),
            _ => throw new ArgumentException(nameof(_openingMepTaskIncoming.OpeningType)),
        };
    }
}
