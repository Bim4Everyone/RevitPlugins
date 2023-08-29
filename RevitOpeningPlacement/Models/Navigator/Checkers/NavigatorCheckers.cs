using System;
using System.Collections.Generic;
using System.Linq;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.Checkers;

namespace RevitOpeningPlacement.Models.Navigator.Checkers {
    /// <summary>
    /// Класс для проверки Revit репозитория на корректность для навигации по заданиям на отверстия
    /// </summary>
    internal class NavigatorCheckers {
        private readonly RevitRepository _revitRepository;
        private List<IChecker> _checkers;


        /// <summary>
        /// Конструктор класса для проверки Revit репозитория на корректность для навигации по заданиям на отверстия
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public NavigatorCheckers(RevitRepository revitRepository) {
            if(revitRepository == null) { throw new ArgumentNullException(nameof(revitRepository)); }

            _revitRepository = revitRepository;
            InitializeCheckers();
        }

        public List<string> GetErrorTexts() {
            return _checkers.Where(item => !item.IsCorrect())
                .Select(item => item.GetErrorMessage())
                .ToList();
        }

        private void InitializeCheckers() {
            _checkers = new List<IChecker> {
                new DuplicatedLinksChecker(_revitRepository),
                new DocsTitlesChecker(_revitRepository)
            };
        }
    }
}
