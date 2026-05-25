using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitRoundingOfAreas.Models.Enums;
using RevitRoundingOfAreas.Models.Interfaces;

namespace RevitRoundingOfAreas.Models.Providers;

internal class ElementsProviderCurrentView : IElementsProvider {
    private readonly RevitRepository _revitRepository;

    public ElementsProviderCurrentView(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }

    public ElementsProviderType Type => ElementsProviderType.CurrentViewProvider;

    public List<SpatialElement> GetSpatialElements(ElementId phaseId) {
        throw new System.NotImplementedException();
    }
}
