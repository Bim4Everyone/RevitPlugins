using System;
using System.Collections.Generic;
using System.Linq;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.Checkers;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.Checkers {
    /// <summary>
    /// Класс для проверки репозитория активного документа ревита на корректность для размещения чистовых отверстий АР
    /// </summary>
    internal class RealOpeningsArChecker : IChecker {
        private readonly RevitRepository _revitRepository;
        private List<IChecker> _checkers;


        /// <summary>
        /// Конструктор класса для проверки репозитория активного документа ревита на корректность для размещения чистовых отверстий АР
        /// </summary>
        public RealOpeningsArChecker(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            InitializeCheckers();
        }


        public string GetErrorMessage() {
            return string.Join(Environment.NewLine, _checkers.Where(item => !item.IsCorrect())
                .Select(item => item.GetErrorMessage()));
        }

        public bool IsCorrect() {
            return _checkers.All(item => item.IsCorrect());
        }


        private void InitializeCheckers() {
            _checkers = new List<IChecker>();
            foreach(OpeningType openingType in Enum.GetValues(typeof(OpeningType))) {
                _checkers.Add(new FamilyAndTypeRealOpeningsArChecker(_revitRepository, openingType));
            }
            // проверка дублированных связей
            _checkers.Add(new DuplicatedLinksChecker(_revitRepository));
        }
    }
}
