using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitMirroredElements.Models;

namespace RevitMirroredElements.ViewModels {
    internal class CategoriesViewModel : BaseViewModel {

        private readonly RevitRepository _revitRepository;
        private readonly CollectionViewSource _filteredCategoriesSource;
        private readonly ObservableCollection<CategoryElement> _allCategories;
        private string _searchText;
        private bool _hasSearchText;
        private bool _allCategoriesSelected;

        public CategoriesViewModel(RevitRepository revitRepository, List<Category> selectedCategories) {
            _revitRepository = revitRepository;

            _allCategories = new ObservableCollection<CategoryElement>(
            _revitRepository.GetCategories()
            .OrderBy(category => category.Name)
            .Select(category => new CategoryElement {
                Id = category.Id,
                Name = category.Name,
                Category = category,
                IsSelected = selectedCategories != null && selectedCategories.Any(sc => sc.Id == category.Id),
            }));

            _filteredCategoriesSource = new CollectionViewSource();
            _filteredCategoriesSource.Filter += FilterCategories;
            _filteredCategoriesSource.Source = _allCategories;

            _hasSearchText = true;

            LoadViewCommand = RelayCommand.Create(LoadView);
        }

        public ICommand LoadViewCommand { get; }

        public ICollectionView FilteredCategories => _filteredCategoriesSource.View;

        public bool HasSearchText {
            get => _hasSearchText;
            private set => RaiseAndSetIfChanged(ref _hasSearchText, value);
        }
        public bool AllCategoriesSelected {
            get => _allCategoriesSelected;
            set {
                RaiseAndSetIfChanged(ref _allCategoriesSelected, value);
                MassSelectedCategories();
            }
        }
        public string SearchText {
            get => _searchText;
            set {
                RaiseAndSetIfChanged(ref _searchText, value);
                HasSearchText = string.IsNullOrEmpty(_searchText);
                UpdateFilterCategories();
            }
        }

        private void FilterCategories(object sender, FilterEventArgs e) {
            if(e.Item is CategoryElement category) {
                e.Accepted = string.IsNullOrEmpty(SearchText) || category.Name.ToLower().Contains(SearchText.ToLower());
            }
        }

        private void MassSelectedCategories() {
            foreach(CategoryElement category in _allCategories) {
                if(FilteredCategories.Contains(category)) {
                    category.IsSelected = _allCategoriesSelected;
                }
            }
            UpdateSelectedCategories();
            UpdateFilterCategories();
        }

        private void UpdateSelectedCategories() {
            foreach(CategoryElement filteredCategory in _allCategories) {
                var currentCategory = _allCategories.FirstOrDefault(c => c.Id == filteredCategory.Id);
                if(currentCategory != null && FilteredCategories.Contains(filteredCategory)) {
                    currentCategory.IsSelected = filteredCategory.IsSelected;
                }
            }
        }

        private void UpdateFilterCategories() {
            _filteredCategoriesSource.View.Refresh();
        }

        public List<Category> GetSelectedCategories() {
            return _allCategories
                .Where(x => x.IsSelected == true)
                .Select(x => x.Category)
                .ToList();
        }

        private void LoadView() {
            UpdateFilterCategories();
        }
    }
}
