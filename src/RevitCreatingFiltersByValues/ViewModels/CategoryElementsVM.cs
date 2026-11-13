using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitCreatingFiltersByValues.Models;

namespace RevitCreatingFiltersByValues.ViewModels;

internal class CategoryElementsVM : BaseViewModel {
    private string _categoryName;
    private Category _categoryInView;
    private ElementId _categoryIdInView;
    private bool _isCheck = false;
    private List<Element> _elementsInView;

    public CategoryElementsVM(CategoryElements categoryElements) {
        CategoryName = categoryElements.CategoryInView.Name;
        CategoryInView = categoryElements.CategoryInView;
        CategoryIdInView = categoryElements.CategoryIdInView;
        ElementsInView = categoryElements.ElementsInView;
    }    

    public string CategoryName {
        get => _categoryName;
        set => RaiseAndSetIfChanged(ref _categoryName, value);
    }    
    
    public Category CategoryInView {
        get => _categoryInView;
        set => RaiseAndSetIfChanged(ref _categoryInView, value);
    }
    
    public ElementId CategoryIdInView {
        get => _categoryIdInView;
        set => RaiseAndSetIfChanged(ref _categoryIdInView, value);
    }
    
    public bool IsCheck {
        get => _isCheck;
        set => RaiseAndSetIfChanged(ref _isCheck, value);
    }
    
    public List<Element> ElementsInView {
        get => _elementsInView;
        set => RaiseAndSetIfChanged(ref _elementsInView, value);
    }
}
