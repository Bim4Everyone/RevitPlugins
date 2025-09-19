using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitAxonometryViews.ViewModels;

namespace RevitAxonometryViews.Models;
internal class CreationViewRules {
    private readonly Document _document;
    private readonly AxonometryConfig _config;

    public CreationViewRules(string selectedCriterion, bool isCombined, RevitRepository revitRepository) {
        _document = revitRepository.Document;
        _config = revitRepository.AxonometryConfig;
        IsCombined = isCombined;
        IsSingle = !isCombined;

        UseSharedSystemName = selectedCriterion == _config.SharedVisSystemName;
        UseSystemName = !UseSharedSystemName;

        Categories = GetCategories();
        FilterParameter = GetFilterParameter();
    }

    public bool IsSingle { get; }
    public bool IsCombined { get; }
    public bool UseSharedSystemName { get; }
    public bool UseSystemName { get; }
    public IList<ElementId> Categories { get; }
    public ElementId FilterParameter { get; }

    /// <summary>
    /// Возвращает имя выбранного параметра наименования системы
    /// </summary>
    public string GetSystemName(HvacSystemViewModel hvacSystem) {
        return UseSharedSystemName ? hvacSystem.SharedName : hvacSystem.SystemName;
    }

    /// <summary>
    /// Возвращает Id параметра для фильтрации
    /// </summary>
    private ElementId GetFilterParameter() {
        return UseSharedSystemName
            ? _config.SystemSharedNameParam.Id
            : new ElementId(_config.SystemNameBuiltInParam);
    }

    /// <summary>
    /// Получает список параметров ФОП_ВИС_Имя системы
    /// </summary>
    private IList<ElementId> GetSharedVisNameCategories() {
        var sharedVisNameParam =
            _document.GetSharedParamBinding(_config.SharedVisSystemName);
        var parameterBinding = sharedVisNameParam.Binding;
        var sharedVisNameCategories = parameterBinding.GetCategories();

        return [.. sharedVisNameCategories.Select(category => category.Id)];
    }

    /// <summary>
    /// В зависимости от выбранного критерия фильтрации вернет или параметры назначенные ФОП_ВИС_Имя системы
    /// или предварительно определенный список категорий
    /// </summary>
    private IList<ElementId> GetCategories() {
        return UseSharedSystemName ?
            GetSharedVisNameCategories() : AxonometryConfig.SystemCategories
                .Select(category => new ElementId(category))
                .ToList();
    }
}
