using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitRoundingOfAreas.Models.Enums;
using RevitRoundingOfAreas.Models.Interfaces;

namespace RevitRoundingOfAreas.Models.Providers;

internal class ElementsProviderAll(RevitRepository revitRepository) : IElementsProvider {
    public ElementsProviderType Type => ElementsProviderType.AllElementsProvider;

    public List<SpatialModel> GetSpatialElements(ElementId phaseId) {
        return revitRepository.GetAllSpatialModels(phaseId);
    }
}
