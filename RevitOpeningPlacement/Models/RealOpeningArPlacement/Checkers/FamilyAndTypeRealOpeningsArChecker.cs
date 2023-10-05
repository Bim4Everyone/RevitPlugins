using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.Checkers {
    /// <summary>
    /// Класс для проверки репозитория активного документа ревита на наличие семейств и типоразмеров чистовых отверстий АР
    /// </summary>
    internal class FamilyAndTypeRealOpeningsArChecker : IChecker {
        private readonly RealOpeningArFamilyChecker _realOpeningFamilyChecker;
        private readonly RealOpeningArTypeChecker _realOpeningTypeChecker;


        /// <summary>
        /// Конструктор класса для проверки репозитория активного документа ревита на наличие семейств и типоразмеров чистовых отверстий АР
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <param name="openingType"></param>
        public FamilyAndTypeRealOpeningsArChecker(RevitRepository revitRepository, OpeningType openingType) {
            _realOpeningFamilyChecker = new RealOpeningArFamilyChecker(revitRepository, openingType);
            _realOpeningTypeChecker = new RealOpeningArTypeChecker(revitRepository, openingType);
        }


        public string GetErrorMessage() {
            return !_realOpeningFamilyChecker.IsCorrect() ? _realOpeningFamilyChecker.GetErrorMessage() : _realOpeningTypeChecker.GetErrorMessage();
        }

        public bool IsCorrect() {
            return _realOpeningFamilyChecker.IsCorrect() && _realOpeningTypeChecker.IsCorrect();
        }
    }
}
