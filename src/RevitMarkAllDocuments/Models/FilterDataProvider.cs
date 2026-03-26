using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Bim4Everyone.RevitFiltration.Controls;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitMarkAllDocuments.Models;

internal class FilterDataProvider : IDataProvider {
    private readonly RevitRepository _revitRepository;
    private readonly Category _category;

    public FilterDataProvider(RevitRepository revitRepository, Category category) {
        _revitRepository = revitRepository;
        _category = category;
    }

    public ICollection<RevitParam> GetParams(ICollection<Category> categories) {
        return _revitRepository.GetSortableParams(_category);
    }

    public ICollection<Category> GetCategories() {
        return [_category];
    }

    public ICollection<Document> GetDocuments() {
        return [_revitRepository.Document];
    }
}
