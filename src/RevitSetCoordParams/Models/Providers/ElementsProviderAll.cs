using System.Collections.Generic;

using RevitSetCoordParams.Models.Enums;
using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.Models.Providers;

internal class ElementsProviderAll : IElementsProvider {
    private readonly RevitRepository _revitRepository;
    private readonly List<RevitCategory> _revitCategories;

    public ElementsProviderAll(RevitRepository revitRepository, List<RevitCategory> revitCategories) {
        _revitRepository = revitRepository;
        _revitCategories = revitCategories;
    }

    public ProviderType Type => ProviderType.AllElementsProvider;

    public ICollection<RevitElement> GetRevitElements() {
        return _revitRepository.GetAllRevitElements(_revitCategories);
    }
}
