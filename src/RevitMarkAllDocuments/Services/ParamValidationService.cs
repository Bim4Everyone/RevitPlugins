using System.Collections.Generic;
using System.Linq;

using dosymep.Bim4Everyone;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.Services;

internal class ParamValidationService {
    public IList<MarkedElement> CheckAreExistParams(IList<RevitParam> paramsToCheck, 
                                                    IList<MarkedElement> elementsToCheck) {
        var elementsWithError = new List<MarkedElement>();

        foreach(var param in paramsToCheck) {
            var elements = CheckIsExistParam(param, elementsToCheck);
            if(elements.Any()) {
                elementsWithError.AddRange(elements);
            }
        }

        return elementsWithError;
    }

    public IList<MarkedElement> CheckIsExistParam(RevitParam paramToCheck, 
                                                  IList<MarkedElement> elementsToCheck) {
        return elementsToCheck
            .Where(x => !x.RevitElement.IsExistsParam(paramToCheck))
            .ToList();
    }

    public IList<MarkedElement> CheckIsReadonlyParam(RevitParam paramToCheck, 
                                                     IList<MarkedElement> elementsToCheck) {
        return elementsToCheck
            .Where(x => x.RevitElement.IsReadOnlyParam(paramToCheck))
            .ToList();
    }
}
