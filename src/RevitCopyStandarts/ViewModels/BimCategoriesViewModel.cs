using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitCopyStandarts.ViewModels;

internal sealed class BimCategoriesViewModel : BaseViewModel {
    private readonly Document _document;
    private readonly Application _application;

    public BimCategoriesViewModel(string mainFolder, Document document, Application application) {
        _document = document;
        _application = application;

        IEnumerable<BimCategoryViewModel> categories = Directory
            .EnumerateFiles(mainFolder, "*.rvt", SearchOption.TopDirectoryOnly)
            .Select(item => new FileInfo(item))
            .GroupBy(item => item.Name.Split('_')[0])
            .Select(item => new BimCategoryViewModel(item.Key, item, _document, _application))
            .OrderBy(item => item.Name);

        BimCategories = new ObservableCollection<BimCategoryViewModel>(categories);
    }

    public ObservableCollection<BimCategoryViewModel> BimCategories { get; set; }
}
