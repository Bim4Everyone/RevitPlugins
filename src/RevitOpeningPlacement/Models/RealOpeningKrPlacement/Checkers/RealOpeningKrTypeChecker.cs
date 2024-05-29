using System;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.Checkers {
    /// <summary>
    /// Класс для проверки наличия типоразмеров чистовых отверстий КР в репозитории активного документа ревита
    /// </summary>
    internal class RealOpeningKrTypeChecker : IChecker {
        private readonly RevitRepository _revitRepository;
        private readonly OpeningType _openingType;


        /// <summary>
        /// Конструктор класса для проверки наличия типоразмеров чистовых отверстий КР в репозитории активного документа ревита
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <param name="openingType"></param>
        public RealOpeningKrTypeChecker(RevitRepository revitRepository, OpeningType openingType) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _openingType = openingType;
        }


        public string GetErrorMessage() {
            return $"У семейства \'{RevitRepository.OpeningRealKrFamilyName[_openingType]}\' отсутствует тип \'{RevitRepository.OpeningRealKrTypeName[_openingType]}\'";
        }

        public bool IsCorrect() {
            return _revitRepository.GetOpeningRealKrType(_openingType) != null;
        }
    }
}
