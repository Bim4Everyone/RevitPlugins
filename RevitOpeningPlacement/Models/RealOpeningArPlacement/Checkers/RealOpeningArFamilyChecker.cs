using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.Checkers {
    /// <summary>
    /// Класс для проверки на наличие в репозитории активного документа ревита семейств чистовых отверстий АР
    /// </summary>
    internal class RealOpeningArFamilyChecker : IChecker {
        private readonly RevitRepository _revitRepository;
        private readonly OpeningType _openingType;

        /// <summary>
        /// Конструктор класса для проверки на наличие в репозитории активного документа ревита семейств чистовых отверстий АР
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <param name="openingType"></param>
        public RealOpeningArFamilyChecker(RevitRepository revitRepository, OpeningType openingType) {
            _revitRepository = revitRepository;
            _openingType = openingType;
        }

        public string GetErrorMessage() => $"В проекте отсутствует семейство \"{RevitRepository.OpeningRealArFamilyName[_openingType]}\".";

        public bool IsCorrect() {
            return _revitRepository.GetOpeningRealArFamily(_openingType) != null;
        }
    }
}
