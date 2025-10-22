using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitSetCoordParams.Models.Enums;
using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.Models.Providers;

internal class ElementsProviderAll : IElementsProvider {
    private readonly RevitRepository _revitRepository;

    public ElementsProviderAll(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }

    public ElementsProviderType Type => ElementsProviderType.AllElementsProvider;

    public ICollection<RevitElement> GetRevitElements(IEnumerable<BuiltInCategory> categories) {
        return _revitRepository.GetAllRevitElements(categories);
    }
}
