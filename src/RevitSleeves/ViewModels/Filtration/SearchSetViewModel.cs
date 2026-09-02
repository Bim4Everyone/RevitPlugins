using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;

using RevitSleeves.Models.Filtration;

namespace RevitSleeves.ViewModels.Filtration;

/// <summary>
/// Набор элементов, попадающих в заданный контекст фильтра (либо не попадающих в него - при инвертировании)
/// </summary>
internal class SearchSetViewModel : BaseViewModel {
    private readonly Models.RevitRepository _revitRepository;
    private readonly ILogicalFilterContext _filterContext;
    private readonly ICollection<DocInfo> _searchTargets;

    public SearchSetViewModel(
        Models.RevitRepository revitRepository,
        ILogicalFilterContext filterContext,
        ICollection<DocInfo> searchTargets,
        bool inverted) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _filterContext = filterContext ?? throw new ArgumentNullException(nameof(filterContext));
        _searchTargets = searchTargets ?? throw new ArgumentNullException(nameof(searchTargets));
        Inverted = inverted;

        Elements = [];
        ShowElementCommand = RelayCommand.Create<ElementViewModel>(ShowElement, CanShowElement);

        Initialize();
    }


    public ICommand ShowElementCommand { get; }

    /// <summary>
    /// True - в набор попадают элементы заданных категорий, которые не проходят правила фильтра
    /// </summary>
    public bool Inverted { get; }

    public ObservableCollection<ElementViewModel> Elements { get; }

    /// <summary>
    /// Возвращает фильтр элементов набора в активном документе
    /// </summary>
    public ElementFilter GetRevitFilter() {
        return _filterContext.GetFilter().Build(_revitRepository.Document, GetOptions());
    }

    /// <summary>
    /// Возвращает категории, заданные в наборе
    /// </summary>
    public ICollection<BuiltInCategory> GetCategories() {
        return _filterContext.SelectedCategories;
    }

    private void Initialize() {
        foreach(var target in _searchTargets) {
            foreach(var element in GetElements(target)) {
                Elements.Add(element);
            }
        }
    }

    private IEnumerable<ElementViewModel> GetElements(DocInfo docInfo) {
        var categories = _filterContext.SelectedCategories.Select(c => new ElementId(c)).ToList();
        var elementFilter = _filterContext.GetFilter().Build(docInfo.Doc, GetOptions());
        return _revitRepository.GetClashRevitRepository()
            .GetFilteredElements(docInfo.Doc, categories, elementFilter)
            .Select(e => new ElementViewModel(new ElementModel(e, docInfo.Transform)));
    }

    private Bim4Everyone.RevitFiltration.Options GetOptions() {
        return Inverted ? FilterBuildOptions.CreateInverted() : FilterBuildOptions.Create();
    }

    private void ShowElement(ElementViewModel element) {
        _revitRepository.GetClashRevitRepository().SelectAndShowElement([element.ElementModel]);
    }

    private bool CanShowElement(ElementViewModel element) {
        return element is not null;
    }
}
