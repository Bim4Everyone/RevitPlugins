using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.Bim4Everyone;

namespace RevitMarkAllDocuments.Models.Filter;

internal class FilterDataProvider {
    private readonly Document _document;
    private readonly IList<FilterableParam> _parameters;
    private readonly Category _category;

    public FilterDataProvider(Document document, Category category, IList<FilterableParam> parameters) {
        _document = document;
        _category = category;
        _parameters = parameters;
    }

    public DataProvider CreateDataProvider() {
        return new DataProvider(GetCategories(), GetParams, GetDocuments());
    }

    private ICollection<RevitParam> GetParams(ICollection<Category> categories) {
        return _parameters
            .Select(x => x.RevitParam)
            .ToList();
    }

    private ICollection<Category> GetCategories() {
        return [_category];
    }

    private ICollection<Document> GetDocuments() {
        return [_document];
    }
}
