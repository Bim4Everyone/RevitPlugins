using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RealOpeningPlacement.Checkers {
    /// <summary>
    /// Класс для проверки на наличие в репозитории активного документа ревита семейств чистовых отверстий
    /// </summary>
    internal class RealOpeningFamilyChecker : IChecker {
        private readonly RevitRepository _revitRepository;
        private readonly OpeningType _openingType;

        /// <summary>
        /// Конструктор класса для проверки на наличие в репозитории активного документа ревита семейств чистовых отверстий
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <param name="openingType"></param>
        public RealOpeningFamilyChecker(RevitRepository revitRepository, OpeningType openingType) {
            _revitRepository = revitRepository;
            _openingType = openingType;
        }

        public string GetErrorMessage() => $"В проекте отсутствует семейство \"{RevitRepository.OpeningRealFamilyName[_openingType]}\".";

        public bool IsCorrect() {
            return _revitRepository.GetOpeningRealFamily(_openingType) != null;
        }
    }
}
