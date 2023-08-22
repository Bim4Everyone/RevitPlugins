using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers {
    internal class FamilyChecker : IChecker {
        private readonly RevitRepository _revitRepository;
        private readonly OpeningType _openingType;

        public FamilyChecker(RevitRepository revitRepository, OpeningType openingType) {
            _revitRepository = revitRepository;
            _openingType = openingType;
        }
        public string GetErrorMessage() => $"В проекте отсутствует семейство \"{RevitRepository.OpeningTaskFamilyName[_openingType]}\".";

        public bool IsCorrect() {
            return _revitRepository.GetOpeningTaskFamily(_openingType) != null;
        }
    }
}
