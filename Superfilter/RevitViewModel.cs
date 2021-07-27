using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.WPF.Commands;

namespace Superfilter {
    internal class RevitViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly Func<RevitRepository, IList<Element>> _getElements;
        private string _name;

        public RevitViewModel(Application application, Document document, Func<RevitRepository, IList<Element>> getElements) {
            _getElements = getElements;
            _revitRepository = new RevitRepository(application, document);

            SelectElements = new RelayCommand(SetSelectedElement, CanSetSelectedElement);

            CategoryViewModels = new ObservableCollection<CategoryViewModel>(GetCategoryViewModels());
        }

        public string Name {
            get => $"{_name} [{CategoryViewModels?.Count ?? 0}]";
            set => _name = value;
        }

        public ICommand SelectElements { get; }
        public ObservableCollection<CategoryViewModel> CategoryViewModels { get; }

        private IEnumerable<CategoryViewModel> GetCategoryViewModels() {
            return _getElements(_revitRepository)
                .GroupBy(item => item.Category, new CategoryComparer())
                .Select(item => new CategoryViewModel(item.Key, item))
                .OrderBy(item => item.Name);
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
