using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitAxonometryViews.Models;
public class CollectorOperator {
    public IList<Element> GetParameterFilterElements(Document document) {
        return new FilteredElementCollector(document).OfClass(typeof(ParameterFilterElement)).ToList();
    }

    public IList<Element> GetElementsByCategory(Document document, BuiltInCategory category) {
        return new FilteredElementCollector(document)
            .OfCategory(category)
            .WhereElementIsNotElementType()
            .ToList();
    }

    public IList<Element> GetElementsByMultiCategory(Document document,
        ICollection<BuiltInCategory> multiCategory) {
        var multiCategoryFilter = new ElementMulticategoryFilter(multiCategory);

        return new FilteredElementCollector(document)
            .WherePasses(multiCategoryFilter)
            .WhereElementIsNotElementType()
            .ToList();
    }
}
