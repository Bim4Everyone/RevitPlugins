using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration;
using Bim4Everyone.RevitFiltration.Controls;

using dosymep.Revit;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.Services;

internal class FiltrationService {
    private readonly IMarkStrategy _markStrategy;
    private readonly Category _category;
    private readonly DocumentService _docService;
    private readonly ILogicalFilter _logicalFilter;
    private readonly FilterOptions _filterOptions;

    public FiltrationService(IMarkStrategy markStrategy, 
                             Category category,
                             DocumentService docService,
                             ILogicalFilterContext logicalFilterContext) {
        _markStrategy = markStrategy;
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
            var elements = _markStrategy.FilterElements(document, _category, filter);

            marksByDocument.Elements = [..elements
                    .Select(x => new MarkedElement(x))];

            markData.MarkDataByDocument.Add(marksByDocument);
        }

        return markData;
    }
}
