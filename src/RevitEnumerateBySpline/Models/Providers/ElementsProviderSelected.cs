using System.Collections.Generic;

using RevitEnumerateBySpline.Models.Enums;
using RevitEnumerateBySpline.Models.Interfaces;

namespace RevitEnumerateBySpline.Models.Providers;

internal class ElementsProviderSelected(RevitRepository revitRepository) : IElementsProvider {
    public ElementsProviderType Type => ElementsProviderType.SelectedElementsProvider;

    public List<SpatialModel> GetSpatialElements() {
        return revitRepository.GetSelectedSpatialModels();
    }
}
