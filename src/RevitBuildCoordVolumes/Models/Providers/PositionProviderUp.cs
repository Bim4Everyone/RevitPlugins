using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models.Providers;

internal class PositionProviderUp : IPositionProvider {
    private readonly RevitRepository _revitRepository;

    public PositionProviderUp(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }
    public PositionProviderType Type => PositionProviderType.UpPosition;

    public XYZ GetPositionSlabElement(SlabElement slabElement) {
        return _revitRepository.GetPositionUp(slabElement);
    }
}
