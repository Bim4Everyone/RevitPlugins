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
using RevitClashDetective.ViewModels.Interfaces;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class FilterViewModel : BaseViewModel, IEquatable<FilterViewModel>, INamedViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly Filter _filter;
        private string _id;
        private string _name;
        private SetViewModel _set;
        private CategoriesInfoViewModel _categoriesInfoViewModel;
        private ObservableCollection<CategoryViewModel> _categories;
        private bool _canSelectCategories;
        private bool? _isAllCategoriesSelected;
        private bool _showOnlySelectedCategories;
        private CollectionViewSource _categoriesView;

        public FilterViewModel(RevitRepository revitRepository) {
            _id = Guid.NewGuid().ToString();
            _revitRepository = revitRepository;
            Name = "Без имени";

            InitializeCategories();
            InitializeSet();

            SelectedCategoriesChangedCommand =
                new RelayCommand(SelectedCategoriesChanged, CanSelectedCategoriesChange);
            RefreshCommand = new RelayCommand(RefreshView);
        }

        public FilterViewModel(RevitRepository revitRepository, Filter filter) {
            _id = Guid.NewGuid().ToString();
            _revitRepository = revitRepository;
            _filter = filter;
            Name = _filter.Name;

            InitializeCategories(_filter.CategoryIds);
            InitializeSet(_filter.Set);

            SelectedObjectCategories = new List<object>(_filter.CategoryIds
                .Select(id => new CategoryViewModel(_revitRepository.GetCategory((BuiltInCategory) id))));

            SelectedCategoriesChangedCommand =
                new RelayCommand(SelectedCategoriesChanged, CanSelectedCategoriesChange);
            RefreshCommand = new RelayCommand(RefreshView);
        }


        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
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
        public bool IsSelectedFilterChanged { get; set; }

        public ICommand RefreshCommand { get; }
        public ICommand SelectedCategoriesChangedCommand { get; }

        public SetViewModel Set {
            get => _set;
            set => this.RaiseAndSetIfChanged(ref _set, value);
        }

        public CollectionViewSource CategoriesView {
            get => _categoriesView;
            set => this.RaiseAndSetIfChanged(ref _categoriesView, value);
        }

        public ObservableCollection<CategoryViewModel> Categories {
            get => _categories;
            set => this.RaiseAndSetIfChanged(ref _categories, value);
        }

        public IEnumerable<CategoryViewModel> SelectedCategories => SelectedObjectCategories?.OfType<CategoryViewModel>() ?? Enumerable.Empty<CategoryViewModel>();

        public List<object> SelectedObjectCategories { get; set; } = new List<object>();

        public void InitializeFilter() {
            IsSelectedFilterChanged = true;
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

            CategoriesView = new CollectionViewSource() { Source = Categories };
            CategoriesView.Filter += SelectedCategoryFilter;
            CategoriesView?.View?.Refresh();

            if(ids != null) {
                SelectedObjectCategories = new List<object>(Categories
                    .Where(item => ids.Any(id => id == item.Category.Id.IntegerValue)));
            }

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

        private void RefreshView(object p) {
            CategoriesView?.View?.Refresh();
        }

        private void SelectedCategoryFilter(object sender, FilterEventArgs e) {
            if(e.Item is CategoryViewModel category && ShowOnlySelectedCategories && !SelectedCategories.Any(item => item.Equals(category))) {
                e.Accepted = false;
            }
        }

        private void SelectedCategoriesChanged(object p) {
            _categoriesInfoViewModel.Categories = new ObservableCollection<CategoryViewModel>(SelectedCategories);
            _categoriesInfoViewModel.InitializeParameters();
            Set.Renew();
        }

        private bool CanSelectedCategoriesChange(object p) {
            if(IsSelectedFilterChanged) {
                IsSelectedFilterChanged = false;
                return false;
            }
            return true;
        }

        public override bool Equals(object obj) {
            return Equals(obj as FilterViewModel);
        }

        public override int GetHashCode() {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(_id);
        }

        public bool Equals(FilterViewModel other) {
            return other != null && _id == other._id;
        }
    }
}