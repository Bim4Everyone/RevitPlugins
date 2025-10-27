using Autodesk.Revit.DB;

using RevitSetCoordParams.Models.Enums;
using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.Models.Providers;
internal class PositionProviderUp : IPositionProvider {
    private readonly RevitRepository _revitRepository;

    public PositionProviderUp(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }
    public PositionProviderType Type => PositionProviderType.UpPositionProvider;

    public XYZ GetPositionElement(RevitElement revitElement) {
        return _revitRepository.GetPositionUp(revitElement);
    }
}
