using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.Services;

internal class SortElementService {
    public IOrderedEnumerable<MarkedElement> SortElements(bool isForType,
                                                          IList<MarkedElement> elements, 
                                                          IList<FilterableParam> sortParams) {
        IOrderedEnumerable<MarkedElement> sortedElements;

        var selectedParamST = sortParams[0].Param.StorageType;

        if(selectedParamST == StorageType.Integer || selectedParamST == StorageType.ElementId) {
            sortedElements = elements.OrderBy(x => x.GetElementWithParam(isForType, sortParams[0]).GetParamValue<int>(sortParams[0].Param));
        } else if(selectedParamST == StorageType.Double) {
            sortedElements = elements.OrderBy(x => x.GetElementWithParam(isForType, sortParams[0]).GetParamValue<double>(sortParams[0].Param));
        } else {
            sortedElements = elements.OrderBy(x => x.GetElementWithParam(isForType, sortParams[0]).GetParamValue<string>(sortParams[0].Param));
        }

        if(sortParams.Count > 1) {
            foreach(var param in sortParams.Skip(1)) {
                var paramST = param.Param.StorageType;

                if(paramST == StorageType.Integer || paramST == StorageType.ElementId) {
                    sortedElements = sortedElements.ThenBy(x => x.GetElementWithParam(isForType, param).GetParamValue<int>(param.Param));
                } else if(paramST == StorageType.Double) {
                    sortedElements = sortedElements.ThenBy(x => x.GetElementWithParam(isForType, param).GetParamValue<double>(param.Param));
                } else {
                    sortedElements = sortedElements.ThenBy(x => x.GetElementWithParam(isForType, param).GetParamValue<string>(param.Param));
                }
            }
        }

        return sortedElements;
    }
}
