using System.Linq;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.Revit;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.Services;

internal class FiltrationService {
    private readonly bool _isMarkForTypes;
    private readonly FilterOptions _filterOptions;

    public FiltrationService(bool isMarkForTypes) {
        _isMarkForTypes = isMarkForTypes;
        _filterOptions = new FilterOptions() { Tolerance = 0 };
    }

    public MarkData FilterElements(MarkData markData, 
                                   Category category,
                                   Document[] documents,
                                   DocumentService docService, 
                                   ILogicalFilterProvider filterProvider) {
        foreach(var document in documents) {
            var marksByDocument = new MarkDataByDocument() {
                DocumentName = docService.GetDocumentFullName(document)
            };

            var filter = filterProvider.GetFilter().GetFilter().Build(document, _filterOptions);

            var elements = new FilteredElementCollector(document)
                .OfCategory(category.GetBuiltInCategory())
                .WhereElementIsNotElementType()
                .WherePasses(filter)
                .ToElements();

            if(_isMarkForTypes) {
                elements = [.. elements
                    .Select(x => x.GetElementType())
                    .GroupBy(x => x.Id)
                    .Select(g => g.First())
                    .Cast<Element>()];
            }

            marksByDocument.Elements = [..elements
                    .Select(x => new MarkedElement(x))];

            markData.MarkDataByDocument.Add(marksByDocument);
        }

        return markData;
    }
}
