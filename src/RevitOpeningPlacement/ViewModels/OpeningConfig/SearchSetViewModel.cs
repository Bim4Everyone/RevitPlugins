using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Filtration;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;
/// <summary>
/// Набор элементов, попадающих в заданный поисковый набор
/// (либо не попадающих в него - при инвертировании)
/// </summary>
internal abstract class SearchSetViewModel : BaseViewModel {
    private protected readonly RevitRepository _revitRepository;
    private protected readonly CategoryFilter _filter;

    protected SearchSetViewModel(RevitRepository revitRepository, CategoryFilter filter, bool inverted) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _filter = filter ?? throw new ArgumentNullException(nameof(filter));
        Inverted = inverted;
        InitializeGrid();
    }

    /// <summary>
    /// Название поискового набора
    /// </summary>
    public string Name => _filter.Name;

    /// <summary>
    /// True - в набор попадают элементы заданных категорий, которые не проходят правила фильтра
    /// </summary>
    public bool Inverted { get; }

    public GridControlViewModel Grid { get; private protected set; }

    /// <summary>
    /// Возвращает фильтр элементов набора в активном документе
    /// </summary>
    public ElementFilter GetRevitFilter() {
        return GetRevitFilter(_revitRepository.Doc);
    }

    /// <summary>
    /// Возвращает категории, заданные в наборе
    /// </summary>
    public ICollection<BuiltInCategory> GetCategories() {
        return _filter.Categories;
    }

    private protected abstract void InitializeGrid();

    /// <summary>
    /// Возвращает фильтр элементов набора в заданном документе
    /// </summary>
    /// <param name="doc">Документ, в котором происходит фильтрация</param>
    private protected ElementFilter GetRevitFilter(Document doc) {
        return _filter.Build(doc, Inverted ? FilterBuildOptions.CreateInverted() : FilterBuildOptions.Create());
    }

    /// <summary>
    /// Возвращает Id категорий, заданных в наборе
    /// </summary>
    private protected ICollection<ElementId> GetCategoryIds() {
        List<ElementId> ids = [];
        foreach(var category in _filter.Categories) {
            ids.Add(new ElementId(category));
        }
        return ids;
    }
}
