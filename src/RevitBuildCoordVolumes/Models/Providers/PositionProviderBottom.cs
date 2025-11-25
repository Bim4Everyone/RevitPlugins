using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models.Providers;

internal class PositionProviderBottom : IPositionProvider {
    private readonly RevitRepository _revitRepository;

    public PositionProviderBottom(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }
    public PositionProviderType Type => PositionProviderType.BottomPosition;

    public XYZ GetPositionSlabElement(SlabElement slabElement) {
        return _revitRepository.GetPositionBottom(slabElement);
    }
}
