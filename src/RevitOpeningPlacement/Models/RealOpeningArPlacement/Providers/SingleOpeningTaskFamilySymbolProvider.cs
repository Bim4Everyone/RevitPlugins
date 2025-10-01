using System;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.Providers;
/// <summary>
/// Класс, предоставляющий типоразмер семейства чистового отверстия по заданию на отверстие для размещения по одному заданию на отверстие
/// </summary>
internal class SingleOpeningTaskFamilySymbolProvider {
    private readonly RevitRepository _revitRepository;
    private readonly Element _host;
    private readonly OpeningMepTaskIncoming _openingMepTaskIncoming;

    /// <summary>
    /// Конструктор класса, предоставляющего типоразмер семейства чистового отверстия по заданию на отверстие для размещения по одному заданию на отверстие
    /// </summary>
    /// <param name="revitRepository">Репозиторий активного документа, в котором будет происходить размещение чистового семейства отверстия</param>
    /// <param name="host">Хост чистового отверстия - стена или перекрытие</param>
    /// <param name="incomingTask">Входящее задание на отверстие</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public SingleOpeningTaskFamilySymbolProvider(RevitRepository revitRepository, Element host, OpeningMepTaskIncoming incomingTask) {
        if(revitRepository is null) { throw new ArgumentNullException(nameof(revitRepository)); }
        if(host is null) { throw new ArgumentNullException(nameof(host)); }
        if(incomingTask is null) { throw new ArgumentNullException(nameof(incomingTask)); }

        _revitRepository = revitRepository;
        _host = host;
        _openingMepTaskIncoming = incomingTask;
    }


    /// <exception cref="ArgumentException">Исключение, если тип проема задания на отверстие не поддерживается</exception>
    public FamilySymbol GetFamilySymbol() {
        if(_host is Wall) {
            return _openingMepTaskIncoming.OpeningType switch {
                OpeningType.WallRectangle => _revitRepository.GetOpeningRealArType(OpeningType.WallRectangle),
                OpeningType.WallRound => _revitRepository.GetOpeningRealArType(OpeningType.WallRound),
                _ => throw new ArgumentException("Тип основы задания на отверстие не является стеной"),
            };
        } else {
            return _host is Floor
                ? _openingMepTaskIncoming.OpeningType switch {
                    OpeningType.FloorRectangle => _revitRepository.GetOpeningRealArType(OpeningType.FloorRectangle),
                    OpeningType.FloorRound => _revitRepository.GetOpeningRealArType(OpeningType.FloorRound),
                    _ => throw new ArgumentException("Тип основы задания на отверстие не является перекрытием"),
                }
                : throw new ArgumentException(nameof(_host));
        }
    }
}
