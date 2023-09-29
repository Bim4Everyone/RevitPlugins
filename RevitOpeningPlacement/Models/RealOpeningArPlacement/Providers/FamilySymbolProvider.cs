using System;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.Providers {
    /// <summary>
    /// Класс, предоставляющий типоразмер семейства чистового отверстия на основе хоста. Использовать для получения типоразмера чистового отверстия для размещения по нескольким заданиям на отверстия
    /// </summary>
    internal class FamilySymbolProvider {
        private readonly RevitRepository _revitRepository;
        private readonly Element _host;


        /// <summary>
        /// Конструктор класса, предоставляющего типоразмер семейства чистового отверстия на основе хоста
        /// </summary>
        /// <param name="revitRepository">Репозиторий активного документа Revit, в котором будет происходить расстановка чистовых отверстий</param>
        /// <param name="host">Хост для чистового отверстия - стена или перекрытие</param>
        /// <exception cref="ArgumentNullException"></exception>
        public FamilySymbolProvider(RevitRepository revitRepository, Element host) {
            if(revitRepository == null) { throw new ArgumentNullException(nameof(revitRepository)); }
            if(host == null) { throw new ArgumentNullException(nameof(host)); }

            _revitRepository = revitRepository;
            _host = host;
        }


        /// <summary>
        /// Возвращает типоразмер семейства чистового отверстия
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public FamilySymbol GetFamilySymbol() {
            if(_host is Wall) {
                return _revitRepository.GetOpeningRealArType(OpeningType.WallRectangle);

            } else if(_host is Floor) {
                return _revitRepository.GetOpeningRealArType(OpeningType.FloorRectangle);
            } else {
                throw new ArgumentException(nameof(_host));
            }
        }
    }
}
