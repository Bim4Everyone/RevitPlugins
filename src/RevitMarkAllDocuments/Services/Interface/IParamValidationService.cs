using RevitMarkAllDocuments.Models;
using System.Collections.Generic;

namespace RevitMarkAllDocuments.Services;

internal interface IParamValidationService {
    IList<WarningElement> CheckAreExistParams(IReadOnlyList<FilterableParam> paramsToCheck,
                                              IReadOnlyList<MarkedElement> elementsToCheck);

    IList<WarningElement> CheckIsExistParam(FilterableParam paramToCheck,
                                            IReadOnlyList<MarkedElement> elementsToCheck);

    IList<WarningElement> CheckIsReadonlyParam(FilterableParam paramToCheck,
                                               IReadOnlyList<MarkedElement> elementsToCheck);
}
