using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF;

using RevitSuperfilter.Comparators;
using RevitSuperfilter.Models;

namespace RevitSuperfilter.ViewModels;

internal sealed class CategoriesViewModel : ObservableObject, IElementIndexList {
    private readonly Dictionary<ElementId, string> _categories = new();
    private readonly Dictionary<string, CategoryViewModel> _categoryVms = new();

    public int Count => _categories.Keys.Count;
    public ObservableCollection<CategoryViewModel> Categories { get; } = [];

    public void Build(IEnumerable<Element> elements) {
        Clear();

        var catsGroup = elements
            .GroupBy(item => item.Category, CategoryNameComparer.Instance)
            .OrderBy(item => item.Key, CategoryNameComparer.Instance);

        foreach(var catGroup in catsGroup) {
            var elementsInCat = catGroup.ToArray();

            foreach(var element in elementsInCat) {
                _categories.Add(element.Id, GetKey(element));
            }

            var categoryViewModel = new CategoryViewModel(catGroup.Key);
            categoryViewModel.Build(elementsInCat);
            
            Categories.Add(categoryViewModel);
            _categoryVms.Add(GetKey(catGroup.Key), categoryViewModel);
        }
    }

    public void Clear() {
        Categories.Clear();
        _categoryVms.Clear();
        _categories.Clear();
    }

    public void Add(Element element) {
        if(_categories.ContainsKey(element.Id)) {
            Remove(element.Id);
        }
        
        var categoryViewModel = GetOrAdd(element);
        categoryViewModel.Add(element);
        _categories.Add(element.Id, GetKey(element));
    }

    public void Remove(ElementId elementId) {
        if(!_categories.TryGetValue(elementId, out string catKey)) {
            return;
        }

        _categories.Remove(elementId);
        if(!_categoryVms.TryGetValue(catKey, out var categoryViewModel)) {
            return;
        }

        categoryViewModel.Remove(elementId);
        if(categoryViewModel.Count == 0) {
            _categoryVms.Remove(catKey);
            Categories.Remove(categoryViewModel);
        }
        
        OnPropertyChanged(nameof(Count));
    }

    private CategoryViewModel GetOrAdd(Element element) {
        if(!_categoryVms.TryGetValue(GetKey(element), out var categoryViewModel)) {
            categoryViewModel = new CategoryViewModel(element.Category);

            Categories.Add(categoryViewModel);
            _categoryVms.Add(GetKey(element), categoryViewModel);
        }

        return categoryViewModel;
    }

    private static string GetKey(Element element) {
        return GetKey(element.Category);
    }
    
    private static string GetKey(Category category) {
        return category?.Name ?? "<null>";
    }
}
