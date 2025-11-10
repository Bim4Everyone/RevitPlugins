using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers;
/// <summary>
/// Класс для проверки наличия описательных параметров в семействах заданий на отверстия. Для проверки наличия семейств в активном документе использовать <see cref="FamilyAndTypeChecker"/>
/// </summary>
internal class FamiliesParametersChecker : IChecker {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localization;


    /// <summary>
    /// Конструктор класса для проверки наличия описательных параметров в семействах заданий на отверстия.
    /// </summary>
    /// <param name="revitRepository">Репозиторий активного документа ревита</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public FamiliesParametersChecker(RevitRepository revitRepository, ILocalizationService localization) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
    }


    public string GetErrorMessage() {
        foreach(OpeningType openingType in Enum.GetValues(typeof(OpeningType))) {
            var family = _revitRepository.GetOpeningTaskFamily(openingType);
            var notExistentParameters = GetNotExistentParameters(_revitRepository, family);

            if(notExistentParameters.Count > 0) {
                return _localization.GetLocalizedString("Validation.MissingParametersInFamily",
                    family.Name,
                    string.Join("\n", notExistentParameters));
            }
        }
        return string.Empty;
    }

    public bool IsCorrect() {
        foreach(OpeningType openingType in Enum.GetValues(typeof(OpeningType))) {
            var family = _revitRepository.GetOpeningTaskFamily(openingType);
            if(!HasAllParameters(_revitRepository, family)) {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Возвращает коллекцию названий параметров, которые отсутствуют у семейства
    /// </summary>
    /// <param name="revitRepository">Репозиторий активного документа Revit, в который загружено семейство</param>
    /// <param name="family">Семейство для проверки</param>
    private ICollection<string> GetNotExistentParameters(RevitRepository revitRepository, Family family) {
        var type = RevitRepository.GetOpeningType(family.Name);
        var paramNames = GetRequiredParameters(type);
        var familyParameterNames = GetFamilySharedParameterNames(revitRepository, family);

        List<string> notExistentParameters = [];
        foreach(string paramName in paramNames) {
            if(!familyParameterNames.Contains(paramName)) {
                notExistentParameters.Add(paramName);
            }
        }
        return notExistentParameters;
    }

    /// <summary>
    /// Проверяет, присутствуют ли в семействе все необходимые параметры
    /// </summary>
    /// <param name="revitRepository">Репозиторий активного документа Revit, в который загружено семейство</param>
    /// <param name="family">Семейство для проверки</param>
    private bool HasAllParameters(RevitRepository revitRepository, Family family) {
        var type = RevitRepository.GetOpeningType(family.Name);
        var paramNames = GetRequiredParameters(type);
        var familyParameterNames = GetFamilySharedParameterNames(revitRepository, family);

        foreach(string paramName in paramNames) {
            if(!familyParameterNames.Contains(paramName)) {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Возвращает коллекцию названий общих параметров документа семейства
    /// </summary>
    /// <param name="revitRepository">Репозиторий активного документа Revit, в который загружено семейство</param>
    /// <param name="family">Семейство, загруженное в активный документ</param>
    private ICollection<string> GetFamilySharedParameterNames(RevitRepository revitRepository, Family family) {
        using var familyDoc = revitRepository.EditFamily(family);
        string[] paramNames = familyDoc.FamilyManager
            .GetParameters()
            .Where(param => param.IsShared)
            .Select(param => param.Definition.Name)
            .ToArray();
        familyDoc.Close(false);
        return paramNames;
    }

    /// <summary>
    /// Возвращает коллекцию названий параметров, которые должны присутствовать у семейства с заданным <see cref="OpeningType">типом отверстия</see>
    /// </summary>
    /// <param name="openingType">Тип отверстия</param>
    private ICollection<string> GetRequiredParameters(OpeningType openingType) {
        // начальный список с названиями параметров, присутствующих во всех семействах
        var paramNames = new List<string> {
            RevitRepository.OpeningDate,
            RevitRepository.OpeningDescription,
            RevitRepository.OpeningMepSystem,
            RevitRepository.OpeningAuthor,
            RevitRepository.OpeningThickness,
            RevitRepository.OpeningIsManuallyPlaced,
            RevitRepository.OpeningOffsetBottom,
            RevitRepository.OpeningOffsetCenter,
            RevitRepository.OpeningOffsetAdsk,
            RevitRepository.OpeningOffsetFromLevelAdsk,
            RevitRepository.OpeningLevelOffsetAdsk,
            RevitRepository.OpeningOffsetAdskOld,
            RevitRepository.OpeningOffsetFromLevelAdskOld,
            RevitRepository.OpeningLevelOffsetAdskOld
        };
        // добавление специфических параметров семейств
        switch(openingType) {
            case OpeningType.FloorRound:
            case OpeningType.WallRound:
                paramNames.Add(RevitRepository.OpeningDiameter);
                break;
            case OpeningType.WallRectangle:
            case OpeningType.FloorRectangle:
                paramNames.Add(RevitRepository.OpeningHeight);
                paramNames.Add(RevitRepository.OpeningWidth);
                break;
        }
        return paramNames;
    }
}
