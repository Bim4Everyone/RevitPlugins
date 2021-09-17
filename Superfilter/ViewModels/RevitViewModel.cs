using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.WPF.Commands;

using Superfilter.Models;
using Superfilter.Views;

namespace Superfilter.ViewModels {
    internal abstract class RevitViewModel : BaseViewModel {
        protected readonly RevitRepository _revitRepository;

        private string _name;
        private string _filter;
        private string _buttonFilterName;
        private bool _currentSelection = true;
        private CategoryViewModel _categoryViewModel;

        public RevitViewModel(Application application, Document document) {
            _revitRepository = new RevitRepository(application, document);

            SelectElements = new RelayCommand(SetSelectedElement, CanSetSelectedElement);
            SelectCategoriesCommand = new RelayCommand(SelectCategories, CanSelectCategories);

            CategoryViewModels = new ObservableCollection<CategoryViewModel>(GetCategoryViewModels());
            CategoryViewModelsView = CollectionViewSource.GetDefaultView(CategoryViewModels);
            CategoryViewModelsView.Filter = item => FilterCategory(item as CategoryViewModel);

            ChangeCurrentSelection();
        }

        public string DisplayData {
            get => _name;
            set => _name = value;
        }

        public CategoryViewModel CategoryViewModel {
            get => _categoryViewModel;
            set {
                _categoryViewModel = value;
                OnPropertyChanged(nameof(ParamsView));
                OnPropertyChanged(nameof(CategoryViewModel));
            }
        }

        public ParamsView ParamsView { 
            get { return CategoryViewModel?.ParamsView; }
        }

        public ICommand SelectElements { get; }
        public ICommand SelectCategoriesCommand { get; }

        public ICollectionView CategoryViewModelsView { get; }
        public ObservableCollection<CategoryViewModel> CategoryViewModels { get; }

        protected abstract IEnumerable<CategoryViewModel> GetCategoryViewModels();

        #region Filter

        public string Filter {
            get => _filter;
            set {
                _filter = value;
                OnPropertyChanged(nameof(Filter));
                CategoryViewModelsView.Refresh();
            }
        }

        public string ButtonFilterName {
            get => _buttonFilterName;
            set {
                _buttonFilterName = value;
                OnPropertyChanged(nameof(ButtonFilterName));
            }
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
            IEnumerable<Element> elements = CategoryViewModels
                .SelectMany(item => item.GetSelectedElements());

            _revitRepository.SetSelectedElements(elements);
        }

        private bool CanSetSelectedElement(object p) {
            return CategoryViewModels.Any(item => item.IsSelected == true || item.IsSelected == null);
        }

        #endregion
    }
}
