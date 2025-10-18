using System.Collections.Generic;

using RevitSetCoordParams.Models.Enums;
using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.Models.Providers;

internal class ElementsProviderCurrentView : IElementsProvider {
    private readonly RevitRepository _revitRepository;
    private readonly List<RevitCategory> _revitCategories;

    public ElementsProviderCurrentView(RevitRepository revitRepository, List<RevitCategory> revitCategories) {
        _revitRepository = revitRepository;
        _revitCategories = revitCategories;
    }

    public ProviderType Type => ProviderType.CurrentViewProvider;

    public ICollection<RevitElement> GetRevitElements() {
        return _revitRepository.GetCurrentViewRevitElements(_revitCategories);
    }
}
