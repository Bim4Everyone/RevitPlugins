using System;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningArPlacement.PointFinders;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.Providers;
/// <summary>
/// Класс, предоставляющий <see cref="IPointFinder"/> для чистового отверстия для размещения по одному заданию на отверстие
/// </summary>
internal class SingleOpeningTaskPointFinderProvider {
    private readonly OpeningMepTaskIncoming _openingMepTaskIncoming;
    private readonly int _rounding;


    /// <summary>
    /// Конструктор класса, предоставляющего <see cref="IPointFinder"/> для чистового отверстия для размещения по одному заданию на отверстие
    /// </summary>
    /// <param name="incomingTask">Входящее задание на отверстие</param>
    /// <param name="rounding">Округление высотной отметки в мм</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public SingleOpeningTaskPointFinderProvider(OpeningMepTaskIncoming incomingTask, int rounding) {
        _openingMepTaskIncoming = incomingTask ?? throw new ArgumentNullException(nameof(incomingTask));
        _rounding = rounding;
    }


    public IPointFinder GetPointFinder() {
        return new SingleOpeningTaskPointFinder(_openingMepTaskIncoming, _rounding);
    }
}
