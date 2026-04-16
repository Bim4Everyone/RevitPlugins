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
    public IOrderedEnumerable<MarkedElement> SortElements(IList<MarkedElement> elements, IList<RevitParam> sortParams) {
        IOrderedEnumerable<MarkedElement> sortedElements;

        var selectedParamST = sortParams[0].StorageType;

        if(selectedParamST == StorageType.Integer || selectedParamST == StorageType.ElementId) {
            sortedElements = elements.OrderBy(x => x.RevitElement.GetParamValue<int>(sortParams[0]));
        } else if(selectedParamST == StorageType.Double) {
            sortedElements = elements.OrderBy(x => x.RevitElement.GetParamValue<double>(sortParams[0]));
        } else {
            sortedElements = elements.OrderBy(x => x.RevitElement.GetParamValue<string>(sortParams[0]));
        }

        if(sortParams.Count > 1) {
            foreach(var param in sortParams.Skip(1)) {
                var paramST = param.StorageType;

                if(paramST == StorageType.Integer || paramST == StorageType.ElementId) {
                    sortedElements = sortedElements.ThenBy(x => x.RevitElement.GetParamValue<int>(param));
                } else if(paramST == StorageType.Double) {
                    sortedElements = sortedElements.ThenBy(x => x.RevitElement.GetParamValue<double>(param));
                } else {
                    sortedElements = sortedElements.ThenBy(x => x.RevitElement.GetParamValue<string>(param));
                }
            }
        }

        return sortedElements;
    }
}
