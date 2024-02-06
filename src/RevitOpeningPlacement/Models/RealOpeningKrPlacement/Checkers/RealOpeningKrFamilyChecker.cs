using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.Checkers {
    /// <summary>
    /// Класс для проверки на наличие в репозитории активного документа ревита семейств чистовых отверстий КР
    /// </summary>
    internal class RealOpeningKrFamilyChecker : IChecker {
        private readonly RevitRepository _revitRepository;
        private readonly OpeningType _openingType;


        /// <summary>
        /// Конструктор класса для проверки на наличие в репозитории активного документа ревита семейств чистовых отверстий КР
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <param name="openingType"></param>
        public RealOpeningKrFamilyChecker(RevitRepository revitRepository, OpeningType openingType) {
            _revitRepository = revitRepository;
            _openingType = openingType;
        }


        public string GetErrorMessage() {
            return $"В проекте отсутствует семейство \'{RevitRepository.OpeningRealKrFamilyName[_openingType]}\'.";
        }

        public bool IsCorrect() {
            return _revitRepository.GetOpeningRealKrFamily(_openingType) != null;
        }
    }
}
