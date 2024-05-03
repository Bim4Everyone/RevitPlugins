using System;
using System.Collections.Generic;
using System.Linq;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.Checkers;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.Checkers {
    /// <summary>
    /// Класс для проверки репозитория активного документа ревита на корректность для размещения чистовых отверстий КР
    /// </summary>
    internal class RealOpeningsKrChecker : IChecker {
        private readonly RevitRepository _revitRepository;
        private List<IChecker> _checkers;


        /// <summary>
        /// Конструктор класса для проверки репозитория активного документа ревита на корректность для размещения чистовых отверстий КР
        /// </summary>
        /// <param name="revitRepository">Репозиторий активного документа</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RealOpeningsKrChecker(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
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
            _checkers = new List<IChecker>() {
                new FamilyAndTypeRealOpeningsKrChecker(_revitRepository, OpeningType.FloorRectangle),
                new FamilyAndTypeRealOpeningsKrChecker(_revitRepository, OpeningType.WallRectangle),
                new FamilyAndTypeRealOpeningsKrChecker(_revitRepository, OpeningType.WallRound),

                new DuplicatedLinksChecker(_revitRepository)
            };
        }
    }
}
