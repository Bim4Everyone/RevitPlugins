using System;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningArPlacement.ParameterGetters;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.Providers;
/// <summary>
/// Класс, предоставляющий <see cref="IParametersGetter"/> для чистового отверстия для размещения по одному заданию на отверстие
/// </summary>
internal class SingleOpeningTaskParameterGettersProvider {
    private readonly OpeningMepTaskIncoming _openingMepTaskIncoming;
    private readonly IPointFinder _pointFinder;
    private readonly int _rounding;


    /// <summary>
    /// Конструктор класса, предоставляющего <see cref="IParametersGetter"/> для чистового отверстия для размещения по одному заданию на отверстие
    /// </summary>
    /// <param name="incomingTask">Входящее задание на отверстие</param>
    /// <param name="pointFinder">Провайдер точки вставки чистового отверстия</param>
    /// <param name="rounding">Округление размеров отверстия в мм</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public SingleOpeningTaskParameterGettersProvider(OpeningMepTaskIncoming incomingTask, IPointFinder pointFinder, int rounding) {
        if(incomingTask is null) { throw new ArgumentNullException(nameof(incomingTask)); }
        if(pointFinder is null) { throw new ArgumentNullException(nameof(pointFinder)); }

        _openingMepTaskIncoming = incomingTask;
        _pointFinder = pointFinder;
        _rounding = rounding;
    }


    /// <exception cref="ArgumentException">Исключение, если тип проема задания на отверстие не поддерживается</exception>
    public IParametersGetter GetParametersGetter() {
        return _openingMepTaskIncoming.OpeningType switch {
            OpeningType.WallRectangle => new SingleRectangleOpeningTaskInWallParameterGetter(_openingMepTaskIncoming, _pointFinder, _rounding),
            OpeningType.FloorRectangle => new SingleRectangleOpeningTaskInFloorParameterGetter(_openingMepTaskIncoming, _rounding),
            OpeningType.WallRound => new SingleRoundOpeningTaskInWallParameterGetter(_openingMepTaskIncoming, _pointFinder, _rounding),
            OpeningType.FloorRound => new SingleRoundOpeningTaskInFloorParameterGetter(_openingMepTaskIncoming, _rounding),
            _ => throw new ArgumentException(nameof(_openingMepTaskIncoming.OpeningType)),
        };
    }
}
