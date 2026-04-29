using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration;
using Bim4Everyone.RevitFiltration.Controls;

using dosymep.Revit;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.Services;

internal class FiltrationService {
    private readonly bool _isMarkForTypes;
    private readonly Category _category;
    private readonly DocumentService _docService;
    private readonly ILogicalFilter _logicalFilter;
    private readonly FilterOptions _filterOptions;

    public FiltrationService(bool isMarkForTypes, 
                             Category category,
                             DocumentService docService,
                             ILogicalFilterContext logicalFilterContext) {
        _isMarkForTypes = isMarkForTypes;
        _category = category;
        _filterOptions = new FilterOptions() { Tolerance = 0 };
        _docService = docService;
        _logicalFilter = logicalFilterContext.GetFilter();
    }

    public MarkData FilterElements(MarkData markData, IReadOnlyList<Document> documents) {
        foreach(var document in documents) {
            var marksByDocument = new MarkDataByDocument() {
                DocumentName = _docService.GetDocumentFullName(document)
            };

            var filter = _logicalFilter.Build(document, _filterOptions);

            var elements = new FilteredElementCollector(document)
                .OfCategory(_category.GetBuiltInCategory())
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
