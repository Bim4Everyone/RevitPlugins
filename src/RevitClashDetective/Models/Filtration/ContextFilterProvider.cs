using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

using RevitClashDetective.Models.Extensions;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.Filtration;

/// <summary>
/// Предоставляет элементы документа, которые попадают в поисковый набор
/// </summary>
internal class ContextFilterProvider : IProvider {
    private readonly ILogicalFilterContext _filterContext;
    private readonly ICollection<ElementId> _elementsToFilter;

    public ContextFilterProvider(
        Document doc,
        ILogicalFilterContext filterContext,
        Transform transform,
        params ElementId[] elementsToFilter) {
        Doc = doc ?? throw new ArgumentNullException(nameof(doc));
        _filterContext = filterContext ?? throw new ArgumentNullException(nameof(filterContext));
        MainTransform = transform ?? throw new ArgumentNullException(nameof(transform));

        _elementsToFilter = elementsToFilter;
    }

    public Document Doc { get; }

    public Transform MainTransform { get; }

    public List<Element> GetElements() {
        return GetFilteredElementCollector()
            .WherePasses(new ElementMulticategoryFilter(_filterContext.SelectedCategories))
            .WherePasses(_filterContext.GetFilter().Build(Doc, FilterBuildOptions.Create()))
            .WhereElementIsNotElementType()
            .Where(item => item.get_Geometry(new Autodesk.Revit.DB.Options()) != null)
            .ToList();
    }

    public List<Solid> GetSolids(Element element) {
        return element.GetSolids();
    }

    private FilteredElementCollector GetFilteredElementCollector() {
        return _elementsToFilter != null && _elementsToFilter.Count > 0
            ? new FilteredElementCollector(Doc, _elementsToFilter)
            : new FilteredElementCollector(Doc);
    }
}
