using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSuperfilter.Models;
using RevitSuperfilter.Views;

namespace RevitSuperfilter.ViewModels {
    internal abstract class RevitViewModel : BaseViewModel {
        protected readonly RevitRepository _revitRepository;

        private string _name;
        private string _filter;
        private string _buttonFilterName;
        private bool _currentSelection = true;
        private CategoryViewModel _categoryViewModel;
        private ViewViewModel _viewViewModel;
        private ObservableCollection<ViewViewModel> _viewViewModels;

        private readonly Delay _delay;

        public RevitViewModel(Application application, Document document) {
            _revitRepository = new RevitRepository(application, document);

            ShowElements = new RelayCommand(ShowElement, CanShowElement);
            SelectElements = new RelayCommand(SetSelectedElement, CanSetSelectedElement);
            SelectCategoriesCommand = new RelayCommand(SelectCategories, CanSelectCategories);

            CategoryViewModels = new ObservableCollection<CategoryViewModel>(GetCategoryViewModels().Where(item => item.Count > 0));
            foreach(var category in CategoryViewModels) {
                category.PropertyChanged += Category_PropertyChanged;
            }

            CategoryViewModelsView = CollectionViewSource.GetDefaultView(CategoryViewModels);
            CategoryViewModelsView.Filter = item => FilterCategory(item as CategoryViewModel);

            ChangeCurrentSelection();

            _delay = new Delay(250, UpdateShowViews);
        }

        public string DisplayData {
            get => _name;
            set => _name = value;
        }

        public CategoryViewModel CategoryViewModel {
            get => _categoryViewModel;
            set {
                this.RaiseAndSetIfChanged(ref _categoryViewModel, value);
                this.RaisePropertyChanged(nameof(ParamsView));
            }
        }

        public ParamsView ParamsView {
            get { return CategoryViewModel?.ParamsView; }
        }

        public ViewViewModel ViewViewModel {
            get => _viewViewModel;
            set => this.RaiseAndSetIfChanged(ref _viewViewModel, value);
        }

        public ObservableCollection<ViewViewModel> ViewViewModels {
            get => _viewViewModels;
            set => this.RaiseAndSetIfChanged(ref _viewViewModels, value);
        }

        public ICommand ShowElements { get; }
        public ICommand SelectElements { get; }
        public ICommand SelectCategoriesCommand { get; }

        public ICollectionView CategoryViewModelsView { get; }
        public ObservableCollection<CategoryViewModel> CategoryViewModels { get; }

        protected abstract IEnumerable<CategoryViewModel> GetCategoryViewModels();

        #region Filter

        public string Filter {
            get => _filter;
            set {
                this.RaiseAndSetIfChanged(ref _filter, value);
                CategoryViewModelsView.Refresh();
            }
        }

        public string ButtonFilterName {
            get => _buttonFilterName;
            set => this.RaiseAndSetIfChanged(ref _buttonFilterName, value);
        }

        private bool FilterCategory(CategoryViewModel category) {
            if(string.IsNullOrEmpty(Filter)) {
                return true;
            }

            return category.DisplayData.IndexOf(Filter, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        #endregion

        #region SelectCategoriesCommand

        private void SelectCategories(object p) {
            ChangeCurrentSelection();

            IEnumerable<CategoryViewModel> categories = CategoryViewModelsView.OfType<CategoryViewModel>();
            foreach(CategoryViewModel category in categories) {
                category.IsSelected = _currentSelection;
            }
        }

        private bool CanSelectCategories(object p) {
            return CategoryViewModelsView.CanFilter;
        }

        private void ChangeCurrentSelection() {
            _currentSelection = !_currentSelection;
            ButtonFilterName = _currentSelection ? "Убрать выделение" : "Выделить всё";
        }

        #endregion

        #region SelectElements

        private void SetSelectedElement(object p) {
            _revitRepository.SetSelectedElements(GetSelectedElements());
        }

        private bool CanSetSelectedElement(object p) {
            return IsSelectedCategory();
        }

        #endregion

        #region ShowElements

        private void ShowElement(object p) {
            _revitRepository.ShowElements(ViewViewModel.Elements);
            _revitRepository.SetSelectedElements(ViewViewModel.Elements);
        }

        private bool CanShowElement(object p) {
            return ViewViewModel != null && IsSelectedCategory();
        }

        #endregion


        private void Category_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName.Equals(nameof(CategoryViewModel.IsSelected))) {
                _delay.Action();
            }
        }

        private bool IsSelectedCategory() {
            return CategoryViewModels.Any(item => item.IsSelected == true || item.IsSelected == null);
        }

        private void UpdateShowViews() {
            var elements = GetSelectedElements().ToList();
            var viewViewModels = elements
                .Where(item => item.OwnerViewId != ElementId.InvalidElementId)
                .GroupBy(item => item.OwnerViewId)
                .Select(item => new ViewViewModel((View) _revitRepository.GetElement(item.Key), item))
                .OrderBy(item => item.Name);

            ViewViewModels = new ObservableCollection<ViewViewModel>(viewViewModels);
            ViewViewModel = ViewViewModels.FirstOrDefault();
        }

        private IEnumerable<Element> GetSelectedElements() {
            IEnumerable<Element> elements = CategoryViewModels
                .SelectMany(item => item.GetSelectedElements());

            IEnumerable<ElementType> elementTypes = elements.OfType<ElementType>();
            var elementIdsFromTypes = elementTypes
                .SelectMany(item => item.GetDependentElements(new ElementIsElementTypeFilter(true)));

            var categories = CategoryViewModels
                .Where(item => item.IsSelected == true || item.IsSelected == null)
                .ToDictionary(item => item.Category.Id);

            var allElements = CategoryViewModels
                .SelectMany(item => item.Elements)
                .ToDictionary(item => item.Id);

            var elementsFromTypes = _revitRepository.GetElements(elementIdsFromTypes)
                .Where(item => item.Category != null)
                .Where(item => categories.ContainsKey(item.Category.Id))
                .Where(item => allElements.ContainsKey(item.Id));

            var selectedElements = elements.Except(elementTypes).Union(elementsFromTypes);
            return selectedElements;
        }
    }

    internal class ViewViewModel {
        public ViewViewModel(View view, IEnumerable<Element> elements) {
            View = view;
            Elements = elements.ToList();
        }

        public View View { get; }
        public List<Element> Elements { get; }

        public string Name {
            get { return View?.Name ?? "Без вида"; }
        }

        public int Count {
            get { return Elements.Count; }
        }

        public override string ToString() {
            return Name;
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
