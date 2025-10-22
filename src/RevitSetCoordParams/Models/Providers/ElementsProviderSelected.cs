using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitSetCoordParams.Models.Enums;
using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.Models.Providers;

internal class ElementsProviderSelected : IElementsProvider {
    private readonly RevitRepository _revitRepository;

    public ElementsProviderSelected(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }

    public ElementsProviderType Type => ElementsProviderType.SelectedElementsProvider;

    public ICollection<RevitElement> GetRevitElements(IEnumerable<BuiltInCategory> categories) {
        return _revitRepository.GetSelectedRevitElements(categories);
    }
}
