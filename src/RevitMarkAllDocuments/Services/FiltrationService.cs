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
                                   ILogicalFilterProvider filterProvider) {
        var docService = new DocumentService();

        foreach(var document in documents) {
            var marksByDocument = new MarkDataByDocument() {
                DocumentName = docService.GetDocumentFullName(document)
            };

            var filter = filterProvider.GetFilter().GetFilter().Build(document, _filterOptions);

            var collector = new FilteredElementCollector(document)
                .OfCategory(category.GetBuiltInCategory());

            FilteredElementCollector collectorByTypes;
            if(_isMarkForTypes) {
                // Если в GUI не выбран ни один параметр для фильтрации, то Bim4Everyone.RevitFiltration
                // возвращает фильтр ElementIsElementTypeFilter(true), а значит типоразмеры не отфильтруются.
                // Для этого случая не надо использовать никакой фильтр.
                if(filter is ElementIsElementTypeFilter) {
                    collectorByTypes = collector
                        .WhereElementIsElementType();
                } else {
                    collectorByTypes = collector
                        .WhereElementIsElementType()
                        .WherePasses(filter);
                }                
            } else {
                collectorByTypes = collector
                    .WhereElementIsNotElementType()
                    .WherePasses(filter);
            }

            marksByDocument.Elements = [..collectorByTypes.ToElements()
                    .Select(x => new MarkedElement(x))];

            markData.MarkDataByDocument.Add(marksByDocument);
        }

        return markData;
    }
}
