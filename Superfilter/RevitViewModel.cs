using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace Superfilter {
    internal class RevitViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        public RevitViewModel(Application application, Document document) {
            _revitRepository = new RevitRepository(application, document);
            CategoryViewModels = new ObservableCollection<CategoryViewModel>(GetCategoryViewModels());
        }

        public ObservableCollection<CategoryViewModel> CategoryViewModels { get; }

        private IEnumerable<CategoryViewModel> GetCategoryViewModels() {
            return _revitRepository.GetElements()
                .GroupBy(item => item.Category)
                .Select(item => new CategoryViewModel(item.Key, item))
                .OrderBy(item => item.Name);
        }
    }
}
