using System.Collections.Generic;

using RevitSetCoordParams.Models.Enums;
using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.Models.Providers;

internal class ElementsProviderSelected : IElementsProvider {
    private readonly RevitRepository _revitRepository;
    private readonly List<RevitCategory> _revitCategories;

    public ElementsProviderSelected(RevitRepository revitRepository, List<RevitCategory> revitCategories) {
        _revitRepository = revitRepository;
        _revitCategories = revitCategories;
    }

    public ProviderType Type => ProviderType.SelectedElementsProvider;

    public ICollection<RevitElement> GetRevitElements() {
        return _revitRepository.GetSelectedRevitElements(_revitCategories);
    }
}
