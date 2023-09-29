using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.Checkers {
    /// <summary>
    /// Класс для проверки наличия типоразмеров чистовых отверстий в репозитории активного документа ревита
    /// </summary>
    internal class RealOpeningTypeChecker : IChecker {
        private readonly RevitRepository _revitRepository;
        private readonly OpeningType _openingType;


        /// <summary>
        /// Конструктор класса для проверки наличия типоразмеров чистовых отверстий в репозитории активного документа ревита
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <param name="openingType"></param>
        public RealOpeningTypeChecker(RevitRepository revitRepository, OpeningType openingType) {
            _revitRepository = revitRepository;
            _openingType = openingType;
        }


        public string GetErrorMessage() => $"У семейства \"{RevitRepository.OpeningRealArFamilyName[_openingType]}\" отсутствует тип \"{RevitRepository.OpeningRealArTypeName[_openingType]}\".";

        public bool IsCorrect() {
            return _revitRepository.GetOpeningRealType(_openingType) != null;
        }
    }
}
