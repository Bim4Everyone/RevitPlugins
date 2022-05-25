using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class FilterViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly Filter _filter;
        private bool _isMassSelectionChanged;
        private string _name;
        private string _filterCategoryName;
        private SetViewModel _set;
        private CategoriesInfoViewModel _categoriesInfoViewModel;
        private CollectionViewSource _categoriesViewSource;
        private ObservableCollection<CategoryViewModel> _categories;
        private ObservableCollection<CategoryViewModel> _selectedCategories;
        private bool _canSelectCategories;
        private bool? _isAllCategoriesSelected;
        private bool _showOnlySelectedCategories;

        public FilterViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            Name = "Без имени";

            InitializeCategories();
            InitializeSet();

            CheckCategoryCommand = new RelayCommand(CheckCategory);
            FilterTextChangedCommand = new RelayCommand(FilterTextChanged);
            SelectedCategoriesChangedCommand =
                new RelayCommand(SelectedCategoriesChanged, p => !_isMassSelectionChanged);
        }

        public FilterViewModel(RevitRepository revitRepository, Filter filter) {
            _revitRepository = revitRepository;
            _filter = filter;
            Name = _filter.Name;

            InitializeCategories(_filter.CategoryIds);
            InitializeSet(_filter.Set);

            SelectedCategories = new ObservableCollection<CategoryViewModel>(_filter.CategoryIds
                .Select(id => new CategoryViewModel(_revitRepository.GetCategory((BuiltInCategory) id))));
            CheckAllCategoriesSelected();

            CheckCategoryCommand = new RelayCommand(CheckCategory);
            FilterTextChangedCommand = new RelayCommand(FilterTextChanged);
            SelectedCategoriesChangedCommand =
                new RelayCommand(SelectedCategoriesChanged, p => !_isMassSelectionChanged);
        }


        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string FilterCategoryName {
            get => _filterCategoryName;
            set => this.RaiseAndSetIfChanged(ref _filterCategoryName, value);
        }

        public bool CanSelectCategories {
            get => _canSelectCategories;
            set => this.RaiseAndSetIfChanged(ref _canSelectCategories, value);
        }

        public bool ShowOnlySelectedCategories { 
            get => _showOnlySelectedCategories; 
            set => this.RaiseAndSetIfChanged(ref _showOnlySelectedCategories, value); 
        }

        public bool? IsAllCategoriesSelected {
            get => _isAllCategoriesSelected;
            set => this.RaiseAndSetIfChanged(ref _isAllCategoriesSelected, value);
        }

        public bool IsInitialized { get; set; }

        public ICommand CheckCategoryCommand { get; }
        public ICommand FilterTextChangedCommand { get; }
        public ICommand SelectedCategoriesChangedCommand { get; }


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

        public void InitializeFilter() {
            if(_filter == null || IsInitialized)
                return;
            Set.Initialize();
            IsInitialized = true;
        }

        private void InitializeSet(Set set = null) {
            Set = new SetViewModel(_revitRepository, _categoriesInfoViewModel, set);
        }

        private void InitializeCategories(IEnumerable<int> ids = null) {
            Categories = new ObservableCollection<CategoryViewModel>(
                _revitRepository.GetCategories()
                    .Select(item => new CategoryViewModel(item))
                    .OrderBy(item => item.Name));

            CategoriesViewSource = new CollectionViewSource() { Source = Categories };
            CategoriesViewSource.Filter += CategoryNameFilter;
            CategoriesViewSource.Filter += SelectedCategoryFilter;
            CategoriesViewSource?.View?.Refresh();

            if(ids != null) {
                foreach(var category in Categories) {
                    category.IsSelected = ids.Any(item => item == category.Category.Id.IntegerValue);
                }
            }

            SelectedCategories = new ObservableCollection<CategoryViewModel>(
                Categories.Where(item => item.IsSelected));

            _categoriesInfoViewModel = new CategoriesInfoViewModel(_revitRepository, SelectedCategories);

            CanSelectCategories = true;
            IsAllCategoriesSelected = false;
        }



        public Filter GetFilter() {
            return new Filter(_revitRepository) {
                Name = Name,
                Set = (Set) Set.GetCriterion(),
                CategoryIds = SelectedCategories.Select(item => item.Category.Id.IntegerValue).ToList()
            };
        }

        private void CheckCategory(object p) {
            _isMassSelectionChanged = true;
            foreach(var category in CategoriesViewSource.View.Cast<CategoryViewModel>()) {
                category.IsSelected = !IsAllCategoriesSelected == true;
            }

            _isMassSelectionChanged = false;
            SelectedCategoriesChangedCommand.Execute(null);
        }

        private void FilterTextChanged(object p) {
            CategoriesViewSource?.View?.Refresh();
            CanSelectCategories = CategoriesViewSource.View.Cast<CategoryViewModel>().Count() > 0;
            CheckAllCategoriesSelected();
        }

        private void CategoryNameFilter(object sender, FilterEventArgs e) {
            if(!string.IsNullOrEmpty(FilterCategoryName) && e.Item is CategoryViewModel category) {
                e.Accepted = category.Name.ToLowerInvariant().Contains(FilterCategoryName.ToLowerInvariant());
            }
        }

        private void SelectedCategoryFilter(object sender, FilterEventArgs e) {
            if(e.Item is CategoryViewModel category && ShowOnlySelectedCategories && !category.IsSelected) {
                e.Accepted = false;
            }
        }


        private void SelectedCategoriesChanged(object p) {
            SelectedCategories = new ObservableCollection<CategoryViewModel>(
                Categories.Where(item => item.IsSelected));
            _categoriesInfoViewModel.Categories = SelectedCategories;
            _categoriesInfoViewModel.InitializeParameters();
            Set.Renew();
            CheckAllCategoriesSelected();
        }

        private void CheckAllCategoriesSelected() {
            var filteredCategories = CategoriesViewSource.View.OfType<CategoryViewModel>();
            if(filteredCategories.All(item => item.IsSelected)) {
                IsAllCategoriesSelected = true;
            } else if(filteredCategories.Any(item => item.IsSelected)) {
                IsAllCategoriesSelected = null;
            } else {
                IsAllCategoriesSelected = false;
            }
        }

        public override bool Equals(object obj) {
            return obj is FilterViewModel model &&
                   Name == model.Name;
        }

        public override int GetHashCode() {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }
    }
}