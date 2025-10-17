using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.ClashDetection;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.ViewModels.ClashDetective.Interfaces;

namespace RevitClashDetective.ViewModels.ClashDetective;
internal class FilterProviderViewModel : BaseViewModel, IProviderViewModel {
    private readonly Filter _filter;

    public FilterProviderViewModel() { }

    public FilterProviderViewModel(Filter filter) {
        _filter = filter;
        Name = filter.Name;
    }

    public string Name { get; }

    public IProvider GetProvider(Document doc, Transform transform) {
        return new FilterProvider(doc, _filter, transform);
    }
}
