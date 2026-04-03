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
    public MarkData SortElements(MarkData markData, IList<RevitParam> sortParams, int startValue) {
        IOrderedEnumerable<MarkedElement> orderedMarkedElements;

        var selectedParamST = sortParams[0].StorageType;

        var allElements = markData.GetAllElements();

        if(selectedParamST == StorageType.Integer || selectedParamST == StorageType.ElementId) {
            orderedMarkedElements = allElements.OrderBy(x => x.Element.GetParamValue<int>(sortParams[0]));
        } else if(selectedParamST == StorageType.Double) {
            orderedMarkedElements = allElements.OrderBy(x => x.Element.GetParamValue<double>(sortParams[0]));
        } else {
            orderedMarkedElements = allElements.OrderBy(x => x.Element.GetParamValue<string>(sortParams[0]));
        }

        if(sortParams.Count > 1) {
            foreach(var param in sortParams.Skip(1)) {
                var paramST = param.StorageType;

                if(paramST == StorageType.Integer || paramST == StorageType.ElementId) {
                    orderedMarkedElements = orderedMarkedElements.ThenBy(x => x.Element.GetParamValue<int>(param));
                } else if(paramST == StorageType.Double) {
                    orderedMarkedElements = orderedMarkedElements.ThenBy(x => x.Element.GetParamValue<double>(param));
                } else {
                    orderedMarkedElements = orderedMarkedElements.ThenBy(x => x.Element.GetParamValue<string>(param));
                }
            }
        }

        foreach(var element in orderedMarkedElements) {
            element.MarkValue = startValue.ToString();
            startValue++;
        }

        return markData;
    }
}
