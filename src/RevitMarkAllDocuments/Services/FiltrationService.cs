using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

using RevitMarkAllDocuments.Models;
using RevitMarkAllDocuments.ViewModels;

namespace RevitMarkAllDocuments.Services;

internal class FiltrationService {
    public IList<Element> FilterElements(Document[] documents, 
                                         ILogicalFilterProvider filterProvider) {
        var allElements = new List<Element>();

        foreach(var document in documents) {
            var filter = filterProvider.GetFilter();

            var elements = new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WherePasses(filter.GetFilter().Build(document, new FilterOptions() { Tolerance = 0 }))
                .ToElements();

            allElements.AddRange(elements);
        }

        return allElements;
    }
}
