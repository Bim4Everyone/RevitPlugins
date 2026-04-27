using System.Collections.Generic;
using System.Linq;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.Services;

internal class ParamValidationService {
    public IList<WarningElement> CheckAreExistParams(bool isForTypes,
                                                    IReadOnlyList<FilterableParam> paramsToCheck, 
                                                    IReadOnlyList<MarkedElement> elementsToCheck) {
        var elementsWithError = new List<WarningElement>();

        foreach(var param in paramsToCheck) {
            var elements = CheckIsExistParam(isForTypes, param, elementsToCheck);
            if(elements.Any()) {
                elementsWithError.AddRange(elements);
            }
        }

        return elementsWithError;
    }

    public IList<WarningElement> CheckIsExistParam(bool isForTypes, 
                                                  FilterableParam paramToCheck, 
                                                  IReadOnlyList<MarkedElement> elementsToCheck) {
        if(isForTypes) {
            return elementsToCheck
                .Where(x => !x.RevitElement.IsExistsParam(paramToCheck.RevitParam))
                .Select(x => new WarningElement {
                    Element = x.RevitElement,
                    ParameterName = paramToCheck.RevitParam.Name,
                })
                .ToList();
        }

        return elementsToCheck
            .Where(x => !x.RevitElement.IsExistsParam(paramToCheck.RevitParam) && 
                !x.RevitElement.GetElementType().IsExistsParam(paramToCheck.RevitParam))
            .Select(x => new WarningElement {
                Element = x.RevitElement,
                ParameterName = paramToCheck.RevitParam.Name,
            })
            .ToList();
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
