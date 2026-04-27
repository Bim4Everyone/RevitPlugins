using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.Services;

internal class SortElementService {
    public IOrderedEnumerable<MarkedElement> SortElements(bool isForType,
                                                          IReadOnlyList<MarkedElement> elements,
                                                          IReadOnlyList<FilterableParam> sortParams) {
        var sortedElements = elements.OrderBy(GetSortKey(isForType, sortParams[0]));

        foreach(var param in sortParams.Skip(1)) {
            sortedElements = sortedElements.ThenBy(GetSortKey(isForType, param));
        }

        return sortedElements;
    }

    private Func<MarkedElement, IComparable> GetSortKey(bool isForType, FilterableParam param) {
        return param.RevitParam.StorageType switch {
            StorageType.Integer => x => 
                x.GetElementWithParam(isForType, param).GetParamValue<int>(param.RevitParam),
            StorageType.ElementId => x => 
                x.GetElementWithParam(isForType, param).GetParamValue<long>(param.RevitParam),
            StorageType.Double => x => 
                x.GetElementWithParam(isForType, param).GetParamValue<double>(param.RevitParam),
            _ => x => 
                x.GetElementWithParam(isForType, param).GetParamValue<string>(param.RevitParam),
        };
    }
}
