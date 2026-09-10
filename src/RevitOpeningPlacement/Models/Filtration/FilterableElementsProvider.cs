using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Extensions;
using RevitClashDetective.Models.Interfaces;

namespace RevitOpeningPlacement.Models.Filtration;
/// <summary>
/// Предоставляет элементы документа, которые попадают в заданный поисковый набор
/// </summary>
internal class FilterableElementsProvider : IProvider {
    private readonly CategoryFilter _filter;
    private readonly ICollection<ElementId> _elementsToFilter;

    public FilterableElementsProvider(
        Document doc,
        CategoryFilter filter,
        Transform transform,
        params ElementId[] elementsToFilter) {
        Doc = doc ?? throw new ArgumentNullException(nameof(doc));
        _filter = filter ?? throw new ArgumentNullException(nameof(filter));
        MainTransform = transform ?? throw new ArgumentNullException(nameof(transform));

        _elementsToFilter = elementsToFilter;
    }

    public Document Doc { get; }

    public Transform MainTransform { get; }

    public List<Element> GetElements() {
        return GetFilteredElementCollector()
            .WhereElementIsNotElementType()
            .WherePasses(new ElementMulticategoryFilter([.. _filter.Categories]))
            .WherePasses(_filter.Build(Doc, FilterBuildOptions.Create()))
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
