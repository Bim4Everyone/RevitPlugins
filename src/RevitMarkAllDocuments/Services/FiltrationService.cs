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
    private readonly FilterOptions _filterOptions;
    public FiltrationService() {
        _filterOptions = new FilterOptions() { Tolerance = 0 };
    }

    public IList<MarkedElement> FilterElements(Document[] documents, 
                                         ILogicalFilterProvider filterProvider) {
        var allElements = new List<MarkedElement>();

        foreach(var document in documents) {
            var filter = filterProvider.GetFilter();

            string documentName = GetCentralServerPath(document);

            var elements = new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WherePasses(filter.GetFilter().Build(document, _filterOptions))
                .ToElements()
                .Select(x => new MarkedElement(x, documentName));

            allElements.AddRange(elements);
        }

        return allElements;
    }

    private string GetCentralServerPath(Document document) {
        if(document.IsWorkshared) {
            var modelPath = document.GetWorksharingCentralModelPath();
            return ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
        } else {
            return document.Title;
        }
    }
}
