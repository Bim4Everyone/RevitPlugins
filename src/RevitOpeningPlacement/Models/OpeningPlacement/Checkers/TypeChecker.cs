using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers {
    internal class TypeChecker : IChecker {
        private readonly RevitRepository _revitRepository;
        private readonly OpeningType _openingType;

        public TypeChecker(RevitRepository revitRepository, OpeningType openingType) {
            _revitRepository = revitRepository;
            _openingType = openingType;
        }
        public string GetErrorMessage() => $"У семейства \"{RevitRepository.OpeningTaskFamilyName[_openingType]}\" отсутствует тип \"{RevitRepository.OpeningTaskTypeName[_openingType]}\".";

        public bool IsCorrect() {
            return _revitRepository.GetOpeningTaskType(_openingType) != null;
        }
    }
}
