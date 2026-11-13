using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.SimpleServices;

namespace RevitValueModifier.Models;
internal class RevitRepository {
    private readonly ILocalizationService _localizationService;

    public RevitRepository(UIApplication uiApplication, ILocalizationService localizationService) {
        UIApplication = uiApplication;
        _localizationService = localizationService;
    }

    public UIApplication UIApplication { get; }

    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

    public Document Document => ActiveUIDocument.Document;

    public List<RevitElement> GetRevitElements() {
        return ActiveUIDocument
            .Selection
            .GetElementIds()
            .Select(id => Document.GetElement(id))
            .Select(e => new RevitElement(e, _localizationService))
            .ToList();
    }

    /// <summary>
    /// Возвращает параметры экземпляра и типа, которые есть хотя бы у одного выбранного элемента,
    /// в том числе параметры, недоступные для фильтров видов (например, "Толщина" у перекрытий)
    /// </summary>
    internal List<RevitParameter> GetParamsForRead(List<RevitElement> revitElements) {
        return revitElements
        .SelectMany(revitElement => revitElement.GetVisibleParameters())
        .GroupBy(parameter => parameter.Id)
        .Select(group => new RevitParameter(group.ToList(), Document))
        .OrderBy(rP => rP.ParamName)
        .ToList();
    }

    /// <summary>
    /// Возвращает параметры, в которые плагин умеет записывать значения:
    /// текст, целое, да/нет и вещественные числа с единицами (число, денежная единица, длина, площадь и т.д.).
    /// Остаются только параметры, которые есть у каждого выбранного элемента (у экземпляра или его типа).
    /// Параметры, доступные только для чтения у всех выбранных элементов, исключаются
    /// </summary>
    internal List<RevitParameter> GetParamsForWrite(List<RevitParameter> parameters, List<RevitElement> revitElements) {
        HashSet<ElementId> commonParamIds = GetCommonParamIds(revitElements);
        return parameters
        .Where(p => p.ValueKind != ParamValueKind.Unsupported
            && !p.IsReadOnly
            && commonParamIds.Contains(p.Id))
        .ToList();
    }

    internal ParamValueParser CreateParamValueParser() {
        return new ParamValueParser(Document, _localizationService);
    }

    /// <summary>
    /// Возвращает Id параметров, которые есть одновременно у всех выбранных элементов
    /// </summary>
    private static HashSet<ElementId> GetCommonParamIds(List<RevitElement> revitElements) {
        HashSet<ElementId> commonParamIds = null;
        foreach(RevitElement revitElement in revitElements) {
            var paramIds = revitElement.GetVisibleParameters().Select(parameter => parameter.Id);
            if(commonParamIds is null) {
                commonParamIds = new HashSet<ElementId>(paramIds);
            } else {
                commonParamIds.IntersectWith(paramIds);
            }
        }
        return commonParamIds ?? [];
    }
}
