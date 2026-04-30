using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.Services;

internal class SortElementService {
    private readonly IMarkStrategy _markStrategy;

    public SortElementService(CategoryContext categoryContext) {
        _markStrategy = categoryContext.GetMarkStrategy();
    }

    public IOrderedEnumerable<MarkedElement> SortElements(IReadOnlyList<MarkedElement> elements,
                                                          IReadOnlyList<FilterableParam> sortParams) {
        var sortedElements = elements.OrderBy(GetSortKey(sortParams[0]));

        foreach(var param in sortParams.Skip(1)) {
            sortedElements = sortedElements.ThenBy(GetSortKey(param));
        }

        return sortedElements;
    }

    private Func<MarkedElement, IComparable> GetSortKey(FilterableParam param) {
        return param.RevitParam.StorageType switch {
            StorageType.Integer => x =>
                _markStrategy.GetElementWithParam(x.RevitElement, param).GetParamValue<int>(param.RevitParam),
            StorageType.ElementId => x =>
                _markStrategy.GetElementWithParam(x.RevitElement, param).GetParamValue<long>(param.RevitParam),
            StorageType.Double => x =>
                _markStrategy.GetElementWithParam(x.RevitElement, param).GetParamValue<double>(param.RevitParam),
            _ => x => 
                _markStrategy.GetElementWithParam(x.RevitElement, param).GetParamValue<string>(param.RevitParam),
        };
    }
}
