using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitTagAllCategories.Models;

namespace RevitTagAllCategories.ViewModels {
    internal class CategoriesWindowVM : BaseViewModel {
        private readonly IList<CategoryViewModel> _allCategories;
        private readonly RevitRepository _revitRepository;

        private ObservableCollection<CategoryViewModel> _categories;
        private CategoryViewModel _selectedCategory;
        private bool _isTypeMarking;
        private string _filterText;

        public CategoriesWindowVM(RevitRepository revitRepository) {
            _revitRepository = revitRepository;

            _allCategories = revitRepository
                .GetFilterableCategories()
                .Where(x => x.CategoryType == CategoryType.Model)
                .Select(c => new CategoryViewModel(c))
                .OrderBy(x => x.Name)
                .ToList();

            _categories = new ObservableCollection<CategoryViewModel>(_allCategories);

            SelectCategoryCommand = new RelayCommand(SelectCategory, CanSelectCategory);
        }

        public ICommand SelectCategoryCommand { get; }
        public ObservableCollection<CategoryViewModel> Categories {
            get => _categories;
            set => RaiseAndSetIfChanged(ref _categories, value);
        }
        public CategoryViewModel SelectedCategory {
            get => _selectedCategory;
            set => RaiseAndSetIfChanged(ref _selectedCategory, value);
        }
        public bool IsTypeMarking {
            get => _isTypeMarking;
            set => RaiseAndSetIfChanged(ref _isTypeMarking, value);
        }
        public string FilterText {
            get => _filterText;
            set {
                RaiseAndSetIfChanged(ref _filterText, value);
                FilterCategories();
            }
        }

        private void SelectCategory(object obj) {
            //MainViewModel mainViewModel =
            //    new MainViewModel(_revitRepository, SelectedCategory, IsTypeMarking);

            //MainWindow mainWindow = new MainWindow(mainViewModel);
            //mainWindow.ShowDialog();
        }

        private bool CanSelectCategory(object p) {
            if(SelectedCategory == null) {
                return false;
            }

            return true;
        }

        private void FilterCategories() {
            if(string.IsNullOrEmpty(FilterText)) {
                Categories = new ObservableCollection<CategoryViewModel>(_allCategories);
            } else {
                var filteredCategories = _allCategories
                    .Where(x => x.Name.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
                Categories = new ObservableCollection<CategoryViewModel>(filteredCategories);
            }
        }
    }
}
