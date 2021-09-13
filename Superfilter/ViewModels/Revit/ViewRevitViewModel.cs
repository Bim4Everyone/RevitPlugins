using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using Superfilter.Models;

namespace Superfilter.ViewModels.Revit {
    internal class ViewRevitViewModel : RevitViewModel {
        public ViewRevitViewModel(Application application, Document document)
            : base(application, document) {
        }

        protected override IEnumerable<CategoryViewModel> GetCategoryViewModels() {
            return _revitRepository.GetElements()
                .Where(item => item.Category != null && item.Category.Parent == null)
                .GroupBy(item => item.Category, new CategoryComparer())
                .Select(item => new CategoryViewModel(item.Key, item));
        }
    }
}
