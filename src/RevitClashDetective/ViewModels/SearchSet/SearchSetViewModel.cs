using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Filtration;

namespace RevitClashDetective.ViewModels.SearchSet;
internal class SearchSetViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly List<ElementModel> _elements = [];

    public SearchSetViewModel(RevitRepository revitRepository,
        ILogicalFilterContext filterContext,
        bool inverted) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        FilterContext = filterContext ?? throw new ArgumentNullException(nameof(filterContext));
        Inverted = inverted;
        InitializeGrid();
    }

    public ILogicalFilterContext FilterContext { get; }

    /// <summary>
    /// True - в набор попадают элементы заданных категорий, которые не проходят правила фильтра
    /// </summary>
    public bool Inverted { get; }

    public GridControlViewModel Grid { get; private set; }

    /// <summary>
    /// Возвращает фильтр элементов набора в активном документе
    /// </summary>
    public ElementFilter GetRevitFilter() {
        return FilterContext.GetFilter().Build(_revitRepository.Doc, GetFilterBuildOptions());
    }

    /// <summary>
    /// Возвращает категории, заданные в наборе
    /// </summary>
    public ICollection<BuiltInCategory> GetCategories() {
        return FilterContext.SelectedCategories;
    }

    private void InitializeGrid() {
        foreach(var docInfo in _revitRepository.DocInfos) {
            _elements.AddRange(FindElements(docInfo));
        }

        Grid = new GridControlViewModel(_revitRepository, _elements);
    }

    /// <summary>
    /// Возвращает элементы заданных в наборе категорий, которые проходят правила фильтра,
    /// либо элементы, попадающие в инвертированный набор.
    /// </summary>
    private IEnumerable<ElementModel> FindElements(DocInfo docInfo) {
        var categories = FilterContext.SelectedCategories.ToList();
        return new FilteredElementCollector(docInfo.Doc)
            .WherePasses(new ElementMulticategoryFilter(categories))
            .WhereElementIsNotElementType()
            .WherePasses(FilterContext.GetFilter().Build(docInfo.Doc, GetFilterBuildOptions()))
            .Where(item => item != null && item.IsValidObject)
            .ToArray()
            .Select(e => new ElementModel(e, docInfo.Transform));
    }

    private Bim4Everyone.RevitFiltration.Options GetFilterBuildOptions() {
        return Inverted ? FilterBuildOptions.CreateInverted() : FilterBuildOptions.Create();
    }
}
