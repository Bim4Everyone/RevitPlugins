using System;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Filtration;
using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.ViewModels.ClashDetective.Interfaces;

namespace RevitClashDetective.ViewModels.ClashDetective;
internal class FilterProviderViewModel : BaseViewModel, IProviderViewModel {
    private readonly NamedFilterContext _namedFilter;

    public FilterProviderViewModel(NamedFilterContext namedFilter) {
        _namedFilter = namedFilter ?? throw new ArgumentNullException(nameof(namedFilter));
    }

    public string Name => _namedFilter.Name;

    public IProvider GetProvider(Document doc, Transform transform) {
        return new ContextFilterProvider(doc, _namedFilter.Context, transform);
    }
}
