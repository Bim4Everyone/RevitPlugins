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
    private readonly ILocalizationService _localizationService;
    
    private readonly Dictionary<ElementId, string> _categoryKeys = new();
    private readonly Dictionary<string, CategoryViewModel> _categories = new();

    public CategoriesViewModel(ILocalizationService localizationService) {
        _localizationService = localizationService;
    }

    public int Count => _categoryKeys.Keys.Count;
    public ObservableCollection<CategoryViewModel> Categories { get; } = [];

    public void Build(IEnumerable<Element> elements) {
        Clear();

        var catsGroup = elements
            .GroupBy(item => item.Category, CategoryNameComparer.Instance)
            .OrderBy(item => item.Key, CategoryNameComparer.Instance);

        foreach(var catGroup in catsGroup) {
            var elementsInCat = catGroup.ToArray();

            foreach(var element in elementsInCat) {
                _categoryKeys.Add(element.Id, GetKey(element));
            }

            var categoryViewModel = new CategoryViewModel(catGroup.Key);
            categoryViewModel.Build(elementsInCat);
            
            Categories.Add(categoryViewModel);
            _categories.Add(GetKey(catGroup.Key), categoryViewModel);
        }
    }

    public void Clear() {
        Categories.Clear();
        _categories.Clear();
        _categoryKeys.Clear();
    }

    public void Add(Element element) {
        if(_categoryKeys.ContainsKey(element.Id)) {
            Remove(element.Id);
        }
        
        var categoryViewModel = GetOrAdd(element);
        categoryViewModel.Add(element);
        _categoryKeys.Add(element.Id, GetKey(element));
    }

    public void Remove(ElementId elementId) {
        if(!_categoryKeys.TryGetValue(elementId, out string catKey)) {
            return;
        }

        _categoryKeys.Remove(elementId);
        if(!_categories.TryGetValue(catKey, out var categoryViewModel)) {
            return;
        }

        categoryViewModel.Remove(elementId);
        if(categoryViewModel.Count == 0) {
            _categories.Remove(catKey);
            Categories.Remove(categoryViewModel);
        }
        
        OnPropertyChanged(nameof(Count));
    }

    private CategoryViewModel GetOrAdd(Element element) {
        if(!_categories.TryGetValue(GetKey(element), out var categoryViewModel)) {
            categoryViewModel = new CategoryViewModel(element.Category);

            Categories.Add(categoryViewModel);
            _categories.Add(GetKey(element), categoryViewModel);
        }

        return categoryViewModel;
    }

    private string GetKey(Element element) {
        return GetKey(element.Category);
    }
    
    private string GetKey(Category category) {
        return category?.Name
               ?? _localizationService.GetLocalizedString("Categories.CategoryKey");
    }
}
