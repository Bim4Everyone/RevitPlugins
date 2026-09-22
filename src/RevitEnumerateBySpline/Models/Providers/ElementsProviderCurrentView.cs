using System.Collections.Generic;

using RevitEnumerateBySpline.Models.Enums;
using RevitEnumerateBySpline.Models.Interfaces;

namespace RevitEnumerateBySpline.Models.Providers;

internal class ElementsProviderCurrentView(RevitRepository revitRepository) : IElementsProvider {
    public ElementsProviderType Type => ElementsProviderType.CurrentViewProvider;

    public List<SpatialModel> GetSpatialElements() {
        return revitRepository.GetActiveViewSpatialModels();
    }
}
