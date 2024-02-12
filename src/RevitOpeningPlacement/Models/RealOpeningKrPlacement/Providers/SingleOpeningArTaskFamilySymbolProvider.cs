using System;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.Providers {
    /// <summary>
    /// Класс, предоставляющий типоразмер семейства чистового отверстия КР по одному заданию на отверстие
    /// </summary>
    internal class SingleOpeningArTaskFamilySymbolProvider {
        private readonly RevitRepository _revitRepository;
        private readonly Element _host;
        private readonly OpeningType _openingTaskType;


        /// <summary>
        /// Конструктор класса, предоставляющего типоразмер семейства чистового отверстия КР по одному заданию на отверстие
        /// </summary>
        /// <param name="revitRepository">Репозиторий активного документа КР, в котором будет происходить размещение чистового семейства отверстия</param>
        /// <param name="host">Хост чистового отверстия - стена или перекрытие</param>
        /// <param name="openingTaskType">Тип проема входящего задания на отверстие</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public SingleOpeningArTaskFamilySymbolProvider(RevitRepository revitRepository, Element host, OpeningType openingTaskType) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            if(host is null) {
                throw new ArgumentNullException(nameof(host));
            }
            if(!((host is Wall) || (host is Floor))) {
                throw new ArgumentException(nameof(host));
            }
            _host = host;
            _openingTaskType = openingTaskType;
        }


        /// <exception cref="ArgumentException"></exception>
        public FamilySymbol GetFamilySymbol() {
            if(_host is Wall) {
                switch(_openingTaskType) {
                    case OpeningType.WallRound:
                    return _revitRepository.GetOpeningRealKrType(OpeningType.WallRound);
                    case OpeningType.WallRectangle:
                    default:
                    return _revitRepository.GetOpeningRealKrType(OpeningType.WallRectangle);
                }
            } else if(_host is Floor) {
                switch(_openingTaskType) {
                    default:
                    return _revitRepository.GetOpeningRealKrType(OpeningType.FloorRectangle);
                }
            } else {
                throw new ArgumentException(nameof(_host));
            }
        }
    }
}
