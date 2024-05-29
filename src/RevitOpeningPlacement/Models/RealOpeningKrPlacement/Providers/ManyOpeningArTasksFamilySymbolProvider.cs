using System;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.Providers {
    /// <summary>
    /// Класс, предоставляющий типоразмер семейства чистового отверстия КР по нескольким заданиям на отверстия от АР
    /// </summary>
    internal class ManyOpeningArTasksFamilySymbolProvider {
        private readonly RevitRepository _revitRepository;
        private readonly Element _host;


        /// <summary>
        /// Конструктор класса, предоставляющего типоразмер семейства чистового отверстия КР по нескольким заданиям на отверстия от АР
        /// </summary>
        /// <param name="revitRepository">Репозиторий активного документа КР</param>
        /// <param name="host">Основа для отверстия КР - стена или перекрытие</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ManyOpeningArTasksFamilySymbolProvider(RevitRepository revitRepository, Element host) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            if(host is null) {
                throw new ArgumentNullException(nameof(host));
            }
            if(!((host is Wall) || (host is Floor))) {
                throw new ArgumentException(nameof(host));
            }
            _host = host;
        }


        public FamilySymbol GetFamilySymbol() {
            if(_host is Wall) {
                return _revitRepository.GetOpeningRealKrType(OpeningType.WallRectangle);
            } else if(_host is Floor) {
                return _revitRepository.GetOpeningRealKrType(OpeningType.FloorRectangle);
            } else {
                throw new ArgumentException(nameof(_host));
            }
        }
    }
}
