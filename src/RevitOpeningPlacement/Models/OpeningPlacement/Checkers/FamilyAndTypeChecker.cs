using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers {
    /// <summary>
    /// Класс для проверки нахождения семейства и его типоразмера в проекте ревит
    /// </summary>
    internal class FamilyAndTypeChecker : IChecker {
        private readonly FamilyChecker _familyChecker;
        private readonly TypeChecker _typeChecker;

        public FamilyAndTypeChecker(RevitRepository revitRepository, OpeningType openingType) {
            _familyChecker = new FamilyChecker(revitRepository, openingType);
            _typeChecker = new TypeChecker(revitRepository, openingType);
        }
        public string GetErrorMessage() {
            return !_familyChecker.IsCorrect() ? _familyChecker.GetErrorMessage() : _typeChecker.GetErrorMessage();
        }

        public bool IsCorrect() {
            return _familyChecker.IsCorrect() && _typeChecker.IsCorrect();
        }
    }
}
