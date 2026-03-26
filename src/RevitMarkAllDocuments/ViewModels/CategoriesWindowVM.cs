using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.ViewModels;

internal class CategoriesWindowVM : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly CategoryContext _categoryContext;
    private readonly ILocalizationService _localizationService;

    private ObservableCollection<CategoryViewModel> _categories;
    private ObservableCollection<CategoryViewModel> _filteredCategories;
    private CategoryViewModel _selectedCategory;
    private string _searchText;
    private string _errorText;
    private bool _isMarkForTypes;

    public CategoriesWindowVM(RevitRepository revitRepository, 
                              CategoryContext context,
                              ILocalizationService localizationService) {
        _revitRepository = revitRepository;
        _categoryContext = context;
        _localizationService = localizationService;

        LoadViewCommand = RelayCommand.Create(LoadView);
        SearchCommand = RelayCommand.Create(ApplySearch);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand AcceptViewCommand { get; }

    public ObservableCollection<CategoryViewModel> Categories {
        get => _categories;
        set => RaiseAndSetIfChanged(ref _categories, value);
    }

    public ObservableCollection<CategoryViewModel> FilteredCategories {
        get => _filteredCategories;
        set => RaiseAndSetIfChanged(ref _filteredCategories, value);
    }

    public CategoryViewModel SelectedCategory {
        get => _selectedCategory;
        set => RaiseAndSetIfChanged(ref _selectedCategory, value);
    }

    public bool IsMarkForTypes {
        get => _isMarkForTypes;
        set => RaiseAndSetIfChanged(ref _isMarkForTypes, value);
    }

    public string SearchText {
        get => _searchText;
        set => RaiseAndSetIfChanged(ref _searchText, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    private void LoadView() {
        _categories = [.. _revitRepository.GetCategories()
            .Select(x => new CategoryViewModel(x))];
        FilteredCategories = [.. _categories.OrderBy(x => x.Name)];
    }

    private void AcceptView() {
        _categoryContext.SelectedCategory = SelectedCategory.Category;
        _categoryContext.IsMarkForTypes = IsMarkForTypes;
    }

    private bool CanAcceptView() {
        if(SelectedCategory == null) {
            ErrorText = _localizationService.GetLocalizedString("CategoriesWindow.NoSelection");
            return false;
        }

        ErrorText = null;
        return true;
    }

    private void ApplySearch() {
        if(string.IsNullOrEmpty(SearchText)) {
            FilteredCategories = new ObservableCollection<CategoryViewModel>(_categories.OrderBy(x => x.Name));
        } else {
            FilteredCategories = new ObservableCollection<CategoryViewModel>(
                Categories.Where(item => item.Name
                        .IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        .OrderBy(x => x.Name));
        }
    }
}
