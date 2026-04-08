using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF;
using dosymep.WPF.ViewModels;

using RevitSuperfilter.Comparators;
using RevitSuperfilter.Services;
using RevitSuperfilter.ViewModels;

namespace RevitSuperfilter.Models;

internal sealed class Superfilter : ObservableObject, IElementIndex {
    private readonly Dictionary<ElementId, Element> _elementsById = new();
    private readonly Dictionary<string, CategoryViewModel> _categories = new();

    public ObservableCollection<CategoryViewModel> Categories { get; } = [];

    public void Build(IEnumerable<Element> elements) {
        Clear();
        
        var catsGroup = elements
            .GroupBy(item => item.Category, CategoryNameComparer.Instance)
            .OrderBy(item => item.Key, CategoryNameComparer.Instance);

        foreach(var catGroup in catsGroup) {
            var elementsInCat = catGroup.ToArray();
            
            foreach(var element in elementsInCat) {
                _elementsById.Add(element.Id, element);
            }
            
            var category = new CategoryViewModel(catGroup.Key);
            category.Build(elementsInCat);
            Categories.Add(category);
        }
    }

    public void Clear() {
        Categories.Clear();
        _categories.Clear();
        _elementsById.Clear();
    }

    public void Add(Element element) {
        if(_elementsById.ContainsKey(element.Id)) {
            Remove(element.Id);
        }

        _elementsById.Add(element.Id, element);
        var categoryViewModel = GetOrAdd(element);
        categoryViewModel.Add(element);
    }

    public void Remove(ElementId elementId) {
        if(!_elementsById.TryGetValue(elementId, out Element element)) {
            return;
        }

        _elementsById.Remove(elementId);
        if(!_categories.TryGetValue(element.Category?.Name ?? "", out var categoryViewModel)) {
            return;
        }

        Categories.Remove(categoryViewModel);
        _categories.Remove(element.Category?.Name ?? "");
    }

    private CategoryViewModel GetOrAdd(Element element) {
        if(!_categories.TryGetValue(element.Category?.Name ?? "", out var categoryViewModel)) {
            categoryViewModel = new CategoryViewModel(element.Category);

            Categories.Add(categoryViewModel);
            _categories[element.Category?.Name ?? ""] = categoryViewModel;
        }

        return categoryViewModel;
    }
}
