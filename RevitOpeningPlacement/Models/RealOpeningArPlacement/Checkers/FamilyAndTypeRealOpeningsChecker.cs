using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.Checkers {
    /// <summary>
    /// Класс для проверки репозитория активного документа ревита на наличие семейств и типоразмеров чистовых отверстий
    /// </summary>
    internal class FamilyAndTypeRealOpeningsChecker : IChecker {
        private readonly RealOpeningFamilyChecker _realOpeningFamilyChecker;
        private readonly RealOpeningTypeChecker _realOpeningTypeChecker;


        /// <summary>
        /// Конструктор класса для проверки репозитория активного документа ревита на наличие семейств и типоразмеров чистовых отверстий
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <param name="openingType"></param>
        public FamilyAndTypeRealOpeningsChecker(RevitRepository revitRepository, OpeningType openingType) {
            _realOpeningFamilyChecker = new RealOpeningFamilyChecker(revitRepository, openingType);
            _realOpeningTypeChecker = new RealOpeningTypeChecker(revitRepository, openingType);
        }


        public string GetErrorMessage() {
            return !_realOpeningFamilyChecker.IsCorrect() ? _realOpeningFamilyChecker.GetErrorMessage() : _realOpeningTypeChecker.GetErrorMessage();
        }

        public bool IsCorrect() {
            return _realOpeningFamilyChecker.IsCorrect() && _realOpeningTypeChecker.IsCorrect();
        }
    }
}
