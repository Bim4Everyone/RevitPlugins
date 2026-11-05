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

    internal List<ElementId> GetCategoryIds(List<RevitElement> revitElements) {
        return revitElements
            .Select(rE => rE.Elem.Category.Id)
            .Distinct()
            .ToList();
    }

    internal List<RevitParameter> GetParamsForRead(List<ElementId> categoryIds, List<RevitElement> revitElements) {
        return ParameterFilterUtilities
        .GetFilterableParametersInCommon(Document, categoryIds)
        .Select(id => new RevitParameter(id, Document, FindSampleParameter(revitElements, id)))
        .OrderBy(rP => rP.ParamName)
        .ToList();
    }

    /// <summary>
    /// Возвращает параметры, в которые плагин умеет записывать значения:
    /// текст, целое, да/нет и вещественные числа с единицами (число, денежная единица, длина, площадь и т.д.)
    /// </summary>
    internal List<RevitParameter> GetParamsForWrite(List<RevitParameter> parameters) {
        return parameters
        .Where(p => p.ValueKind != ParamValueKind.Unsupported)
        .ToList();
    }

    internal ParamValueParser CreateParamValueParser() {
        return new ParamValueParser(Document, _localizationService);
    }

    private static Parameter FindSampleParameter(List<RevitElement> revitElements, ElementId paramId) {
        return revitElements
            .Select(revitElement => revitElement.FindParameter(paramId))
            .FirstOrDefault(parameter => parameter is not null);
    }
}
