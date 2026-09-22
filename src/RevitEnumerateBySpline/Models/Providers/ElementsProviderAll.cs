using System.Collections.Generic;

using RevitEnumerateBySpline.Models.Enums;
using RevitEnumerateBySpline.Models.Interfaces;

namespace RevitEnumerateBySpline.Models.Providers;

internal class ElementsProviderAll(RevitRepository revitRepository) : IElementsProvider {
    public ElementsProviderType Type => ElementsProviderType.AllElementsProvider;

    public List<SpatialModel> GetSpatialElements() {
        return revitRepository.GetAllSpatialModels();
    }
}
