using System.Collections.Generic;
using System.Linq;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.Services;

internal class ParamValidationService : IParamValidationService {
    private readonly IMarkStrategy _markStrategy;

    public ParamValidationService(CategoryContext categoryContext) {
        _markStrategy = categoryContext.GetMarkStrategy();
    }

    public IList<WarningElement> CheckAreExistParams(IReadOnlyList<FilterableParam> paramsToCheck, 
                                                     IReadOnlyList<MarkedElement> elementsToCheck) {
        var elementsWithError = new List<WarningElement>();

        foreach(var param in paramsToCheck) {
            var elements = CheckIsExistParam(param, elementsToCheck);
            if(elements.Any()) {
                elementsWithError.AddRange(elements);
            }
        }

        return elementsWithError;
    }

    public IList<WarningElement> CheckIsExistParam(FilterableParam paramToCheck, 
                                                   IReadOnlyList<MarkedElement> elementsToCheck) {
        return _markStrategy.CheckIsExistParam(paramToCheck, elementsToCheck);
    }

    public IList<WarningElement> CheckIsReadonlyParam(FilterableParam paramToCheck, 
                                                      IReadOnlyList<MarkedElement> elementsToCheck) {
        return elementsToCheck
            .Where(x => x.RevitElement.IsReadOnlyParam(paramToCheck.RevitParam))
            .Select(x => new WarningElement {
                Element = x.RevitElement,
                ParameterName = paramToCheck.RevitParam.Name,
            })
            .ToList();
    }
}
