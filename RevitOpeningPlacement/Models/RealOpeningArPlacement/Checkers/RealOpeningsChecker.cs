using System;
using System.Collections.Generic;
using System.Linq;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.Checkers;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.Checkers {
    /// <summary>
    /// Класс для проверки репозитория активного документа ревита на корректность для размещения чистовых отверстий
    /// </summary>
    internal class RealOpeningsChecker {
        private readonly RevitRepository _revitRepository;
        private List<IChecker> _checkers;


        /// <summary>
        /// Конструктор класса для проверки репозитория активного документа ревита на корректность для размещения чистовых отверстий
        /// </summary>
        public RealOpeningsChecker(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            InitializeCheckers();
        }

        public ICollection<string> GetErrorTexts() {
            return _checkers.Where(item => !item.IsCorrect())
                .Select(item => item.GetErrorMessage())
                .ToList();
        }

        private void InitializeCheckers() {
            _checkers = new List<IChecker>();
            foreach(OpeningType openingType in Enum.GetValues(typeof(OpeningType))) {
                _checkers.Add(new FamilyAndTypeRealOpeningsChecker(_revitRepository, openingType));
            }
            // проверка дублированных связей
            _checkers.Add(new DuplicatedLinksChecker(_revitRepository));
        }
    }
}
