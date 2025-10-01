using System;
using System.Collections.Generic;
using System.Linq;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.Checkers;

namespace RevitOpeningPlacement.Models.Navigator.Checkers;
/// <summary>
/// Класс для проверки Revit репозитория на корректность для навигации по заданиям на отверстия
/// </summary>
internal class NavigatorCheckers {
    private readonly RevitRepository _revitRepository;
    private List<IChecker> _checkers;


    /// <summary>
    /// Конструктор класса для проверки Revit репозитория на корректность для навигации по заданиям на отверстия
    /// </summary>
    /// <param name="revitRepository">Репозиторий активного документа ревита</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public NavigatorCheckers(RevitRepository revitRepository) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        InitializeCheckers();
    }

    public List<string> GetErrorTexts() {
        return _checkers.Where(item => !item.IsCorrect())
            .Select(item => item.GetErrorMessage())
            .ToList();
    }

    private void InitializeCheckers() {
        _checkers = [
            new DuplicatedLinksChecker(_revitRepository),
            new DocsTitlesChecker(_revitRepository)
        ];
    }
}
