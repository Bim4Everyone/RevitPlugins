using System;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers {
    /// <summary>
    /// Класс для проверки Revit репозитория на наличие дублированных связей
    /// </summary>
    internal class DuplicatedLinksChecker : IChecker {
        private readonly RevitRepository _revitRepository;


        /// <summary>
        /// Конструктор класса для проверки Revit репозитория на наличие дублированных связей
        /// </summary>
        /// <param name="revitRepository"></param>
        public DuplicatedLinksChecker(RevitRepository revitRepository) {
            if(revitRepository is null) { throw new ArgumentNullException(nameof(revitRepository)); }

            _revitRepository = revitRepository;
        }


        public string GetErrorMessage() {
            return $"Следующие связи дублируются: {string.Join("; ", _revitRepository.GetDuplicatedLinksNames())}.";
        }

        public bool IsCorrect() {
            return _revitRepository.GetDuplicatedLinksNames().Count == 0;
        }
    }
}
