using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitMarkAllDocuments.Models;

internal interface IMarkStrategy {
    IList<WarningElement> CheckIsExistParam(FilterableParam paramToCheck, IReadOnlyList<MarkedElement> elementsToCheck);
    Element GetElementWithParam(Element element, FilterableParam param);
}
