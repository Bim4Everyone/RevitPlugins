using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

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
        private readonly Delay _delay;
        private int _selectedCategoryCount;
        private string _id;
        private string _name;
        private SetViewModel _set;
        private CategoriesInfoViewModel _categoriesInfoViewModel;
        private ObservableCollection<CategoryViewModel> _categories;
        private bool _canSelectCategories;
        private bool? _isAllCategoriesSelected;
        private bool _showOnlySelectedCategories;
        private bool _isMassSelection;


        public FilterViewModel(RevitRepository revitRepository) {
            _id = Guid.NewGuid().ToString();
            _revitRepository = revitRepository;
            Name = "Без имени";

            InitializeCategories();
            InitializeSet();


            _delay = new Delay(250, SelectedCategoriesChanged);
        }

        public FilterViewModel(RevitRepository revitRepository, Filter filter) {
            _id = Guid.NewGuid().ToString();
            _revitRepository = revitRepository;
            _filter = filter;
            Name = _filter.Name;

            InitializeCategories(_filter.CategoryIds);
            InitializeSet(_filter.Set);


            _delay = new Delay(250, SelectedCategoriesChanged);
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

        public bool IsMassSelection { 
            get => _isMassSelection; 
            set => this.RaiseAndSetIfChanged(ref _isMassSelection, value);
        }

        public bool? IsAllCategoriesSelected {
            get => _isAllCategoriesSelected;
            set => this.RaiseAndSetIfChanged(ref _isAllCategoriesSelected, value);
        }

        public bool IsInitialized { get; set; }
        public bool IsSelectedFilterChanged { get; set; }

        public ICommand SelectedCategoriesChangedCommand { get; }

        public SetViewModel Set {
            get => _set;
            set => this.RaiseAndSetIfChanged(ref _set, value);
        }

        public ObservableCollection<CategoryViewModel> Categories {
            get => _categories;
            set => this.RaiseAndSetIfChanged(ref _categories, value);
        }

        public IEnumerable<CategoryViewModel> SelectedCategories => Categories?.Where(item => item.IsSelected) ?? Enumerable.Empty<CategoryViewModel>();

        public ObservableCollection<object> VisibleItems { get; set; }

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

            foreach(var category in Categories) {
                category.PropertyChanged += Category_PropertyChanged;
            }

            if(ids != null) {
                foreach(var category in Categories
                    .Where(item => ids.Any(id => id == item.Category.Id.IntegerValue))) {
                    category.IsSelected = true;
                }
            }

            _selectedCategoryCount = SelectedCategories.Count();
            _categoriesInfoViewModel = new CategoriesInfoViewModel(_revitRepository, SelectedCategories);

            CanSelectCategories = true;
            IsAllCategoriesSelected = false;
        }

        private void Category_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if(e.PropertyName.Equals(nameof(CategoryViewModel.IsSelected))){
                _delay.Action();
            }
        }

        public Filter GetFilter() {
            return new Filter(_revitRepository) {
                Name = Name,
                Set = (Set) Set.GetCriterion(),
                CategoryIds = SelectedCategories.Select(item => item.Category.Id.IntegerValue).ToList()
            };
        }

        private void SelectedCategoryFilter(object sender, FilterEventArgs e) {
            if(e.Item is CategoryViewModel category && ShowOnlySelectedCategories && !SelectedCategories.Any(item => item.Equals(category))) {
                e.Accepted = false;
            }
        }

        private void SelectedCategoriesChanged() {
            _categoriesInfoViewModel.Categories = new ObservableCollection<CategoryViewModel>(SelectedCategories);
            _categoriesInfoViewModel.InitializeParameters();
            Set.Renew();
            _selectedCategoryCount = SelectedCategories.Count();
        }

        private bool CanSelectedCategoriesChanged(object p) {
            if(IsMassSelection && !AllItemsHasCommonSelection())
                return false;
            IsMassSelection = false;
            return SelectedCategories.Count() != _selectedCategoryCount;
        }

        private bool AllItemsHasCommonSelection() {
            return VisibleItems.All(item => ((CategoryViewModel) item).IsSelected) || VisibleItems.All(item => !((CategoryViewModel) item).IsSelected);
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


    internal class Delay {
        private readonly Action _action;
        private readonly DispatcherTimer _timer;

        public Delay(int interval, Action action) {
            _action = action;

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, interval);
            _timer.Tick += _timer_Tick;
        }

        private void _timer_Tick(object sender, EventArgs e) {
            _timer.Stop();
            _action();
        }

        public void Action() {
            _timer.Stop();
            _timer.Start();
        }
    }
}