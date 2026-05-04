using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.Bim4Everyone;

namespace RevitMarkAllDocuments.Models;

internal class FilterDataProvider : IDataProvider {
    private readonly Document _document;
    private readonly IList<FilterableParam> _parameters;
    private readonly Category _category;

    public FilterDataProvider(Document document, Category category, IList<FilterableParam> parameters) {
        _document = document;
        _category = category;
        _parameters = parameters;
    }

    public ICollection<RevitParam> GetParams(ICollection<Category> categories) {
        return _parameters
            .Select(x => x.RevitParam)
            .ToList();
    }

    public ICollection<Category> GetCategories() {
        return [_category];
    }

    public ICollection<Document> GetDocuments() {
        return [_document];
    }
}
