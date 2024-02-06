using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.Checkers {
    /// <summary>
    /// Класс для проверки репозитория активного документа ревита на наличие семейств и типоразмеров чистовых отверстий КР
    /// </summary>
    internal class FamilyAndTypeRealOpeningsKrChecker : IChecker {
        private readonly RealOpeningKrFamilyChecker _realOpeningKrFamilyChecker;
        private readonly RealOpeningKrTypeChecker _realOpeningKrTypeChecker;


        /// <summary>
        /// Конструктор класса для проверки репозитория активного документа ревита на наличие семейств и типоразмеров чистовых отверстий КР
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <param name="openingType"></param>
        public FamilyAndTypeRealOpeningsKrChecker(RevitRepository revitRepository, OpeningType openingType) {
            if(revitRepository is null) { throw new System.ArgumentNullException(nameof(revitRepository)); }
            _realOpeningKrFamilyChecker = new RealOpeningKrFamilyChecker(revitRepository, openingType);
            _realOpeningKrTypeChecker = new RealOpeningKrTypeChecker(revitRepository, openingType);
        }


        public string GetErrorMessage() {
            return !_realOpeningKrFamilyChecker.IsCorrect() ? _realOpeningKrFamilyChecker.GetErrorMessage() : _realOpeningKrTypeChecker.GetErrorMessage();
        }

        public bool IsCorrect() {
            return _realOpeningKrFamilyChecker.IsCorrect() && _realOpeningKrTypeChecker.IsCorrect();
        }
    }
}
