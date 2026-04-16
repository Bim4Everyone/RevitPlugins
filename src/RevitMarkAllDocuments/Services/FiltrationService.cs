using System.Linq;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.Services;

internal class FiltrationService {
    private readonly FilterOptions _filterOptions;
    public FiltrationService() {
        _filterOptions = new FilterOptions() { Tolerance = 0 };
    }

    public MarkData FilterElements(MarkData markData, Document[] documents, ILogicalFilterProvider filterProvider) {
        var docService = new DocumentService();

        foreach(var document in documents) {
            var marksByDocument = new MarkDataByDocument() {
                DocumentName = docService.GetDocumentFullName(document)
            };

            var filter = filterProvider.GetFilter();

            marksByDocument.Elements = new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .WherePasses(filter.GetFilter().Build(document, _filterOptions))
                .ToElements()
                .Select(x => new MarkedElement(x))
                .ToList();

            markData.MarkDataByDocument.Add(marksByDocument);
        }

        return markData;
    }
}
