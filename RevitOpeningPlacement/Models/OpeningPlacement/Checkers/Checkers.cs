using System;
using System.Collections.Generic;
using System.Linq;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers {
    internal class Checkers {
        private readonly RevitRepository _revitRepository;
        private List<IChecker> _checkers;

        public Checkers(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            InitializeCheckers();
        }

        public List<string> GetErrorTexts() {
            return _checkers.Where(item => !item.IsCorrect())
                .Select(item => item.GetErrorMessage())
                .ToList();
        }

        private void InitializeCheckers() {
            _checkers = new List<IChecker>();
            foreach(OpeningType openingType in Enum.GetValues(typeof(OpeningType))) {
                _checkers.Add(new FamilyAndTypeChecker(_revitRepository, openingType));
            }
        }
    }

    internal class FamilyChecker : IChecker {
        private readonly RevitRepository _revitRepository;
        private readonly OpeningType _openingType;

        public FamilyChecker(RevitRepository revitRepository, OpeningType openingType) {
            _revitRepository = revitRepository;
            _openingType = openingType;
        }
        public string GetErrorMessage() => $"В проекте отсутствует семейство \"{RevitRepository.FamilyName[_openingType]}\".";

        public bool IsCorrect() {
            return _revitRepository.GetFamily(_openingType) != null;
        }
    }

    internal class TypeChecker : IChecker {
        private readonly RevitRepository _revitRepository;
        private readonly OpeningType _openingType;

        public TypeChecker(RevitRepository revitRepository, OpeningType openingType) {
            _revitRepository = revitRepository;
            _openingType = openingType;
        }
        public string GetErrorMessage() => $"У семейства \"{RevitRepository.FamilyName[_openingType]}\" отсутствует тип \"{RevitRepository.TypeName[_openingType]}\".";

        public bool IsCorrect() {
            return _revitRepository.GetOpeningType(_openingType) != null;
        }
    }

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
