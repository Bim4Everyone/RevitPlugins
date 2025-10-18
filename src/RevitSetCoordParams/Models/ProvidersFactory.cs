using System.Collections.Generic;

using RevitSetCoordParams.Models.Enums;
using RevitSetCoordParams.Models.Interfaces;
using RevitSetCoordParams.Models.Providers;

namespace RevitSetCoordParams.Models;
internal class ProvidersFactory {

    public IElementsProvider GetElementsProvider(RevitRepository revitRepository, ProviderType type, List<RevitCategory> revitCategories) {
        return type switch {
            ProviderType.AllElementsProvider => new ElementsProviderAll(revitRepository, revitCategories),
            ProviderType.CurrentViewProvider => new ElementsProviderCurrentView(revitRepository, revitCategories),
            ProviderType.SelectedElementsProvider => new ElementsProviderSelected(revitRepository, revitCategories),
            _ => new ElementsProviderAll(revitRepository, revitCategories)
        };
    }

    public IElementsProvider GetFileProvider(RevitRepository revitRepository, ProviderType type, string fileName) {
        return type switch {
            ProviderType.CurrentFileProvider => new FileProviderCurrent(revitRepository, fileName),
            ProviderType.CoordFileProvider => new FileProviderCoord(revitRepository, fileName),
            ProviderType.SelectedLinkFileProvider => new FileProviderLink(revitRepository, fileName),
            _ => new FileProviderCoord(revitRepository, fileName)
        };
    }

    public IPositionProvider GetPositionProvider(RevitRepository revitRepository, ProviderType type) {
        return type switch {
            ProviderType.CenterPositionProvider => new PositionProviderCenter(revitRepository),
            ProviderType.BottomPositionProvider => new PositionProviderBottom(revitRepository),
            _ => new PositionProviderCenter(revitRepository)
        };
    }
}
