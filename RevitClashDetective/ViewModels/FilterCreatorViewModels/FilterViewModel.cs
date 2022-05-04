using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class FilterViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private string _name;
        private CategoriesInfoViewModel _categoriesInfoViewModel;
        private ObservableCollection<CategoryViewModel> _categories;
        private CollectionViewSource _categoriesViewSource;
        private string _filterCategoryName;
        private ObservableCollection<CategoryViewModel> _selectedCategories;
        private SetViewModel _set;

        public FilterViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            Name = "Без имени";
            InitializeCategories();
            SelectedcategoriesChangedCommand = new RelayCommand(SelectedCategoriesChanged);
            FilterTextChagedCommand = new RelayCommand(FilterTextChanged);
            CheckCategoryCommand = new RelayCommand(CheckCategory);
            InitializeSet();
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string FilterCategoryName {
            get => _filterCategoryName;
            set => this.RaiseAndSetIfChanged(ref _filterCategoryName, value);
        }

        public ICommand SelectedcategoriesChangedCommand { get; }
        public ICommand FilterTextChagedCommand { get; }
        public ICommand CheckCategoryCommand { get; }


        public CollectionViewSource CategoriesViewSource {
            get => _categoriesViewSource;
            set => this.RaiseAndSetIfChanged(ref _categoriesViewSource, value);
        }

        public SetViewModel Set {
            get => _set;
            set => this.RaiseAndSetIfChanged(ref _set, value);
        }

        public ObservableCollection<CategoryViewModel> Categories {
            get => _categories;
            set => this.RaiseAndSetIfChanged(ref _categories, value);
        }

        public ObservableCollection<CategoryViewModel> SelectedCategories {
            get => _selectedCategories;
            set => this.RaiseAndSetIfChanged(ref _selectedCategories, value);
        }

        public void InitializeCategories() {
            Categories = new ObservableCollection<CategoryViewModel>(
                _revitRepository.GetCategories()
                .Select(item => new CategoryViewModel(item)));
            CategoriesViewSource = new CollectionViewSource() { Source = Categories };
            CategoriesViewSource.Filter += CategoryNameFilter;
            CategoriesViewSource?.View?.Refresh();
            SelectedCategories = new ObservableCollection<CategoryViewModel>(
               Categories.Where(item => item.IsSelected));
            _categoriesInfoViewModel = new CategoriesInfoViewModel(_revitRepository, SelectedCategories);
        }

        public override bool Equals(object obj) {
            return obj is FilterViewModel model &&
                   Name == model.Name;
        }

        public override int GetHashCode() {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }

        private void CategoryNameFilter(object sender, FilterEventArgs e) {
            if(!string.IsNullOrEmpty(FilterCategoryName) && e.Item is CategoryViewModel category) {
                e.Accepted = category.Name.ToLowerInvariant().Contains(FilterCategoryName.ToLowerInvariant());
            }
        }

        private void SelectedCategoriesChanged(object p) {
            SelectedCategories = new ObservableCollection<CategoryViewModel>(
                Categories.Where(item => item.IsSelected));
            _categoriesInfoViewModel.Categories = SelectedCategories;
            _categoriesInfoViewModel.InitializeParameters();
        }

        private void FilterTextChanged(object p) {
            CategoriesViewSource?.View?.Refresh();
        }

        private void InitializeSet() {
            Set = new SetViewModel(_revitRepository, _categoriesInfoViewModel);
        }

        private void CheckCategory(object p) {
            foreach(var category in Categories) {
                category.IsSelected = !(bool) p;
            }
        }
    }
}