using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitMarkAllDocuments.Models;

internal class MarkTypeStrategy : IMarkStrategy {
    public IList<WarningElement> CheckIsExistParam(FilterableParam paramToCheck, 
                                                   IReadOnlyList<MarkedElement> elementsToCheck) {
        return elementsToCheck
            .Where(x => !x.RevitElement.IsExistsParam(paramToCheck.RevitParam))
            .Select(x => new WarningElement {
                Element = x.RevitElement,
                ParameterName = paramToCheck.RevitParam.Name,
            })
            .ToList();
    }

    public IList<Element> FilterElements(Document document, Category category, ElementFilter filter) {
        return [.. new FilteredElementCollector(document)
            .OfCategory(category.GetBuiltInCategory())
            .WhereElementIsNotElementType()
            .WherePasses(filter)
            .ToElements()
            .Select(x => x.GetElementType())
            .GroupBy(x => x.Id)
            .Select(g => g.First())
            .Cast<Element>()];   
    }
}
