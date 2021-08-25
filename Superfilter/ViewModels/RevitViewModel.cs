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

namespace Superfilter.ViewModels {
    internal class RevitViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly Func<RevitRepository, IList<Element>> _getElements;
        private string _name;
        private string _filter;
        private bool _currentSelection = true;
        private string _buttonFilterName;
        private CategoryViewModel _categoryViewModel;

        public RevitViewModel(Application application, Document document, Func<RevitRepository, IList<Element>> getElements) {
            _getElements = getElements;
            _revitRepository = new RevitRepository(application, document);

            SelectElements = new RelayCommand(SetSelectedElement, CanSetSelectedElement);
            SelectCategoriesCommand = new RelayCommand(SelectCategories, CanSelectCategories);

            CategoryViewModels = new ObservableCollection<CategoryViewModel>(GetCategoryViewModels());
            CategoryViewModelsView = CollectionViewSource.GetDefaultView(CategoryViewModels);
            CategoryViewModelsView.Filter = item => FilterCategory(item as CategoryViewModel);

            ChangeCurrentSelection();
        }

        public string Name {
            get => $"{_name} [{CategoryViewModels?.Count ?? 0}]";
            set => _name = value;
        }

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

        public CategoryViewModel CategoryViewModel {
            get => _categoryViewModel;
            set {
                _categoryViewModel = value;
                OnPropertyChanged(nameof(CategoryViewModel));
            }
        }

        public ICommand SelectElements { get; }
        public ICommand SelectCategoriesCommand { get; }

        public ICollectionView CategoryViewModelsView { get; }
        public ObservableCollection<CategoryViewModel> CategoryViewModels { get; }

        private IEnumerable<CategoryViewModel> GetCategoryViewModels() {
            return _getElements(_revitRepository)
                .Where(item => item.Category != null)
                .GroupBy(item => item.Category, new CategoryComparer())
                .Where(item => item.Key?.Parent == null)
                .Select(item => new CategoryViewModel(item.Key, item))
                .OrderBy(item => item.Name);
        }

        private bool FilterCategory(CategoryViewModel category) {
            if(string.IsNullOrEmpty(Filter)) {
                return true;
            }

            return category.Name.IndexOf(Filter, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private void SelectCategories(object p) {
            ChangeCurrentSelection();

            IEnumerable<CategoryViewModel> categories = CategoryViewModelsView.OfType<CategoryViewModel>();
            foreach(CategoryViewModel category in categories) {
                category.Selected = _currentSelection;
            }
        }

        private void ChangeCurrentSelection() {
            _currentSelection = !_currentSelection;
            ButtonFilterName = _currentSelection ? "Выделить всё" : "Убрать выделение";
        }

        private bool CanSelectCategories(object p) {
            return CategoryViewModelsView.CanFilter;
        }

        private void SetSelectedElement(object p) {
            IEnumerable<Element> elements = CategoryViewModels.Where(item => item.Selected).SelectMany(item => item.Elements);
            _revitRepository.SetSelectedElements(elements);
        }

        private bool CanSetSelectedElement(object p) {
            return CategoryViewModels.Any(item => item.Selected);
        }
    }
}
