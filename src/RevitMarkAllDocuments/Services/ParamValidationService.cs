using System.Collections.Generic;
using System.Linq;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.Services;

internal class ParamValidationService {
    public IList<MarkedElement> CheckAreExistParams(bool isForTypes,
                                                    IList<FilterableParam> paramsToCheck, 
                                                    IList<MarkedElement> elementsToCheck) {
        var elementsWithError = new List<MarkedElement>();

        foreach(var param in paramsToCheck) {
            var elements = CheckIsExistParam(isForTypes, param, elementsToCheck);
            if(elements.Any()) {
                elementsWithError.AddRange(elements);
            }
        }

        return elementsWithError;
    }

    public IList<MarkedElement> CheckIsExistParam(bool isForTypes, 
                                                  FilterableParam paramToCheck, 
                                                  IList<MarkedElement> elementsToCheck) {
        if(isForTypes) {
            return elementsToCheck
                .Where(x => !x.RevitElement.IsExistsParam(paramToCheck.RevitParam))
                .ToList();
        }

        return elementsToCheck
            .Where(x => !x.RevitElement.IsExistsParam(paramToCheck.RevitParam) && 
                !x.RevitElement.GetElementType().IsExistsParam(paramToCheck.RevitParam))
            .ToList();
    }

    public IList<MarkedElement> CheckIsReadonlyParam(FilterableParam paramToCheck, 
                                                     IList<MarkedElement> elementsToCheck) {
        return elementsToCheck
            .Where(x => x.RevitElement.IsReadOnlyParam(paramToCheck.RevitParam))
            .ToList();
    }
}
