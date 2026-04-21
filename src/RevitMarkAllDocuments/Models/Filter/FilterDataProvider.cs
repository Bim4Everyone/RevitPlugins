using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.Bim4Everyone;

namespace RevitMarkAllDocuments.Models;

internal class FilterDataProvider : IDataProvider {
    private readonly RevitRepository _revitRepository;
    private readonly IList<FilterableParam> _parameters;
    private readonly Category _category;

    public FilterDataProvider(RevitRepository revitRepository, IList<FilterableParam> parameters, Category category) {
        _revitRepository = revitRepository;
        _parameters = parameters;
        _category = category;
    }

    public ICollection<RevitParam> GetParams(ICollection<Category> categories) {
        return _parameters
            .Select(x => x.Param)
            .ToList();
    }

    public ICollection<Category> GetCategories() {
        return [_category];
    }

    public ICollection<Document> GetDocuments() {
        return [_revitRepository.Document];
    }
}
