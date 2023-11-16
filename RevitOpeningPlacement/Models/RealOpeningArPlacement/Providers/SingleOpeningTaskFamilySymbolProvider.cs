using System;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.Providers {
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
        /// <exception cref="ArgumentNullException"></exception>
        public SingleOpeningTaskFamilySymbolProvider(RevitRepository revitRepository, Element host, OpeningMepTaskIncoming incomingTask) {
            if(revitRepository is null) { throw new ArgumentNullException(nameof(revitRepository)); }
            if(host is null) { throw new ArgumentNullException(nameof(host)); }
            if(incomingTask is null) { throw new ArgumentNullException(nameof(incomingTask)); }

            _revitRepository = revitRepository;
            _host = host;
            _openingMepTaskIncoming = incomingTask;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public FamilySymbol GetFamilySymbol() {
            if(_host is Wall) {
                switch(_openingMepTaskIncoming.OpeningType) {
                    case OpeningType.WallRectangle:
                    return _revitRepository.GetOpeningRealArType(OpeningType.WallRectangle);
                    case OpeningType.WallRound:
                    return _revitRepository.GetOpeningRealArType(OpeningType.WallRound);
                    default:
                    throw new ArgumentException("Тип основы задания на отверстие не является стеной");
                }
            } else if(_host is Floor) {
                switch(_openingMepTaskIncoming.OpeningType) {
                    case OpeningType.FloorRectangle:
                    return _revitRepository.GetOpeningRealArType(OpeningType.FloorRectangle);
                    case OpeningType.FloorRound:
                    return _revitRepository.GetOpeningRealArType(OpeningType.FloorRound);
                    default:
                    throw new ArgumentException("Тип основы задания на отверстие не является перекрытием");
                }
            } else {
                throw new ArgumentException(nameof(_host));
            }
        }
    }
}
