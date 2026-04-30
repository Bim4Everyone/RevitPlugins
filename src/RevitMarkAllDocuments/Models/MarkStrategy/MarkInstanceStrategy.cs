using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.Models;

internal class MarkInstanceStrategy : IMarkStrategy {
    public IList<WarningElement> CheckIsExistParam(FilterableParam paramToCheck,
                                                   IReadOnlyList<MarkedElement> elementsToCheck) {
        return elementsToCheck
            .Where(x => !x.RevitElement.IsExistsParam(paramToCheck.RevitParam) &&
                !x.RevitElement.GetElementType().IsExistsParam(paramToCheck.RevitParam))
            .Select(x => new WarningElement {
                Element = x.RevitElement,
                ParameterName = paramToCheck.RevitParam.Name,
            })
            .ToList();
    }

    //public Element GetElementWithParam(Element element, FilterableParam param) {
    //    return param.IsTypeParam ? element.GetElementType() : element;
    //}
}
