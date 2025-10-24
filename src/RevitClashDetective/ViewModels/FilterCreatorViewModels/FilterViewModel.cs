using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Threading;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels;
internal class FilterViewModel : BaseViewModel, IEquatable<FilterViewModel>, INamedEntity {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localization;
    private readonly Filter _filter;
    private readonly Delay _delay;
    private readonly string _id;
    private string _name;
    private SetViewModel _set;
    private CategoriesInfoViewModel _categoriesInfoViewModel;
    private ObservableCollection<CategoryViewModel> _allCategories;
    private bool _allCategoriesSelected;
    private bool _hideUnselectedCategories;
    private CollectionViewSource _categories;
    private string _categoriesFilter;


    public FilterViewModel(RevitRepository revitRepository,
        ILocalizationService localization,
        Filter filter = null) {
        _id = Guid.NewGuid().ToString();
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _filter = filter;
        Name = _filter?.Name;

        _delay = new Delay(250, SelectedCategoriesChanged);

        InitializeCategories(_filter?.CategoryIds);
        InitializeSet(_filter?.Set);

        PropertyChanged += CategoriesFilterPropertyChanged;
    }

    public bool AllCategoriesSelected {
        get => _allCategoriesSelected;
        set {
            if(_allCategoriesSelected != value) {
                RaiseAndSetIfChanged(ref _allCategoriesSelected, value);
                foreach(var category in AllCategories) {
                    category.IsSelected = value;
                }
            }
        }
    }

    public string CategoriesFilter {
        get => _categoriesFilter;
        set => RaiseAndSetIfChanged(ref _categoriesFilter, value);
    }

    public bool HideUnselectedCategories {
        get => _hideUnselectedCategories;
        set => RaiseAndSetIfChanged(ref _hideUnselectedCategories, value);
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public bool IsInitialized { get; set; }

    public SetViewModel Set {
        get => _set;
        set => RaiseAndSetIfChanged(ref _set, value);
    }

    public CollectionViewSource Categories {
        get => _categories;
        private set => RaiseAndSetIfChanged(ref _categories, value);
    }

    public ObservableCollection<CategoryViewModel> AllCategories {
        get => _allCategories;
        private set => RaiseAndSetIfChanged(ref _allCategories, value);
    }

    public void InitializeFilter() {
        if(_filter == null || IsInitialized) {
            return;
        }

        Set.Initialize();
        IsInitialized = true;
    }

    public override bool Equals(object obj) {
        return Equals(obj as FilterViewModel);
    }

    public override int GetHashCode() {
        return 539060726 + EqualityComparer<string>.Default.GetHashCode(_id);
    }

    public bool Equals(FilterViewModel other) {
        if(other is null) { return false; }
        return ReferenceEquals(this, other) || _id == other._id;
    }

    public Filter GetFilter() {
        return new Filter(_revitRepository) {
            Name = Name,
            Set = (Set) Set.GetCriterion(),
            CategoryIds = GetSelectedCategories().Select(item => item.Category.Id).ToList()
        };
    }

    private IEnumerable<CategoryViewModel> GetSelectedCategories() {
        return AllCategories?.Where(item => item.IsSelected) ?? [];
    }

    private void CategoriesFilterPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(CategoriesFilter)
            || e.PropertyName == nameof(HideUnselectedCategories)) {
            Categories?.View.Refresh();
        }
    }

    private void InitializeSet(Set set = null) {
        Set = new SetViewModel(_revitRepository, _localization, _categoriesInfoViewModel, set);
    }

    private void InitializeCategories(IEnumerable<ElementId> categoryIds = null) {
        AllCategories = new ObservableCollection<CategoryViewModel>(GetCategoriesViewModels(_revitRepository));
        Categories = new CollectionViewSource() { Source = AllCategories };
        Categories.Filter += CategoriesFilterHandler;

        if(categoryIds != null) {
            foreach(var category in AllCategories
                .Where(item => categoryIds.Any(id => id == item.Category.Id))) {
                category.IsSelected = true;
            }
        }

        foreach(var category in AllCategories) {
            category.PropertyChanged += CategoryPropertyChanged;
        }

        _categoriesInfoViewModel = new CategoriesInfoViewModel(_revitRepository, _localization, GetSelectedCategories());
    }

    private void CategoriesFilterHandler(object sender, FilterEventArgs e) {
        if(e.Item is CategoryViewModel category) {
            if(HideUnselectedCategories && !category.IsSelected) {
                e.Accepted = false;
                return;
            }
            if(!string.IsNullOrWhiteSpace(CategoriesFilter)) {
                string str = CategoriesFilter.ToLower();
                e.Accepted = category.Name.ToLower().Contains(str);
                return;
            }
            e.Accepted = true;
        }
    }

    private ICollection<CategoryViewModel> GetCategoriesViewModels(RevitRepository revitRepository) {
        return revitRepository is null
            ? throw new ArgumentNullException(nameof(revitRepository))
            : (ICollection<CategoryViewModel>) revitRepository
            .GetCategories()
            .Select(item => new CategoryViewModel(item))
            .OrderBy(item => item.Name)
            .ToList();
    }

    private void CategoryPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
        if(e.PropertyName.Equals(nameof(CategoryViewModel.IsSelected))) {
            _delay.Action();
        }
    }

    private void SelectedCategoriesChanged() {
        _categoriesInfoViewModel.Categories = new ObservableCollection<CategoryViewModel>(GetSelectedCategories());
        _categoriesInfoViewModel.InitializeParameters();
        Set.Renew();
    }
}

internal class Delay {
    private readonly Action _action;
    private readonly DispatcherTimer _timer;

    public Delay(int interval, Action action) {
        _action = action;

        _timer = new DispatcherTimer {
            Interval = new TimeSpan(0, 0, 0, 0, interval)
        };
        _timer.Tick += OnTimerTick;
    }

    private void OnTimerTick(object sender, EventArgs e) {
        _timer.Stop();
        _action();
    }

    public void Action() {
        _timer.Stop();
        _timer.Start();
    }
}
