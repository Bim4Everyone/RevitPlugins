using System;
using System.Collections.Generic;
using System.Linq;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers {
    /// <summary>
    /// Класс - хранитель проверок <see cref="RevitOpeningPlacement.Models.RevitRepository._document">файла ревита</see> на корректность для расстановки заданий на отверстия
    /// </summary>
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
            // добавление проверок на наличие семейств заданий на отверстия и их типоразмеров
            foreach(OpeningType openingType in Enum.GetValues(typeof(OpeningType))) {
                _checkers.Add(new FamilyAndTypeChecker(_revitRepository, openingType));
            }
            // проверка дублированных связей
            _checkers.Add(new DuplicatedLinksChecker(_revitRepository));
            // проверка на соответствие названий файлов бим стандарту
            _checkers.Add(new DocsTitlesChecker(_revitRepository));
        }
    }
}
