using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitSetCoordParams.Models.Enums;
using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.Models.Providers;

internal class ElementsProviderCurrentView : IElementsProvider {
    private readonly RevitRepository _revitRepository;

    public ElementsProviderCurrentView(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }

    public ElementsProviderType Type => ElementsProviderType.CurrentViewProvider;

    public ICollection<RevitElement> GetRevitElements(IEnumerable<BuiltInCategory> categories) {
        return _revitRepository.GetCurrentViewRevitElements(categories);
    }
}
