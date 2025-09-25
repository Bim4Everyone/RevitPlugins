using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using RevitSuperfilter.Models;

namespace RevitSuperfilter.ViewModels.Revit;

internal class ViewRevitViewModel : RevitViewModel {
    public ViewRevitViewModel(Application application, Document document)
        : base(application, document) {
    }

    protected override IEnumerable<CategoryViewModel> GetCategoryViewModels() {
        return _revitRepository.GetElements()
            .Where(item => item.Category != null)
            .GroupBy(item => item.Category, new CategoryComparer())
            .Select(item => new CategoryViewModel(item.Key, item, _revitRepository))
            .OrderBy(item => item.DisplayData);
    }
}
