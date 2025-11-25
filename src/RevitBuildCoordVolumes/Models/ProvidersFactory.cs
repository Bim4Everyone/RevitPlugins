using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Providers;

namespace RevitBuildCoordVolumes.Models;

internal class ProvidersFactory {
    /// <summary>
    /// Метод получения провайдера позиции по Enum
    /// </summary>
    public IPositionProvider GetPositionProvider(RevitRepository revitRepository, PositionProviderType type) {
        return type switch {
            PositionProviderType.BottomPosition => new PositionProviderBottom(revitRepository),
            PositionProviderType.UpPosition => new PositionProviderUp(revitRepository),
            _ => new PositionProviderUp(revitRepository)
        };
    }
}
