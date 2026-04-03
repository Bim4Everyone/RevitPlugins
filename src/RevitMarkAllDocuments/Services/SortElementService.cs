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
        IOrderedEnumerable<MarkedElement> sortedMarkedElements;

        var selectedParamST = sortParams[0].StorageType;

        if(selectedParamST == StorageType.Integer || selectedParamST == StorageType.ElementId) {
            sortedMarkedElements = elements.OrderBy(x => x.Element.GetParamValue<int>(sortParams[0]));
        } else if(selectedParamST == StorageType.Double) {
            sortedMarkedElements = elements.OrderBy(x => x.Element.GetParamValue<double>(sortParams[0]));
        } else {
            sortedMarkedElements = elements.OrderBy(x => x.Element.GetParamValue<string>(sortParams[0]));
        }

        if(sortParams.Count > 1) {
            foreach(var param in sortParams.Skip(1)) {
                var paramST = param.StorageType;

                if(paramST == StorageType.Integer || paramST == StorageType.ElementId) {
                    sortedMarkedElements = sortedMarkedElements.ThenBy(x => x.Element.GetParamValue<int>(param));
                } else if(paramST == StorageType.Double) {
                    sortedMarkedElements = sortedMarkedElements.ThenBy(x => x.Element.GetParamValue<double>(param));
                } else {
                    sortedMarkedElements = sortedMarkedElements.ThenBy(x => x.Element.GetParamValue<string>(param));
                }
            }
        }

        return sortedMarkedElements;
    }
}
