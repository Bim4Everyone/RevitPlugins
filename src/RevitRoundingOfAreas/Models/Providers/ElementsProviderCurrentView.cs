using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitRoundingOfAreas.Models.Enums;
using RevitRoundingOfAreas.Models.Interfaces;

namespace RevitRoundingOfAreas.Models.Providers;

internal class ElementsProviderCurrentView(RevitRepository revitRepository) : IElementsProvider {
    public ElementsProviderType Type => ElementsProviderType.CurrentViewProvider;

    public List<SpatialModel> GetSpatialElements(ElementId phaseId) {
        return revitRepository.GetActiveViewSpatialModels(phaseId);
    }
}
