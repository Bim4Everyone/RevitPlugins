using Autodesk.Revit.DB;

using RevitSetCoordParams.Models.Enums;
using RevitSetCoordParams.Models.Interfaces;
using RevitSetCoordParams.Models.Providers;

namespace RevitSetCoordParams.Models;
internal class ProvidersFactory {

    public IElementsProvider GetElementsProvider(RevitRepository revitRepository, ElementsProviderType type) {
        return type switch {
            ElementsProviderType.AllElementsProvider => new ElementsProviderAll(revitRepository),
            ElementsProviderType.CurrentViewProvider => new ElementsProviderCurrentView(revitRepository),
            ElementsProviderType.SelectedElementsProvider => new ElementsProviderSelected(revitRepository),
            _ => new ElementsProviderAll(revitRepository)
        };
    }

    public IFileProvider GetFileProvider(RevitRepository revitRepository, Document document) {
        return new FileProvider(revitRepository, document);
    }

    public IPositionProvider GetPositionProvider(RevitRepository revitRepository, PositionProviderType type) {
        return type switch {
            PositionProviderType.CenterPositionProvider => new PositionProviderCenter(revitRepository),
            PositionProviderType.BottomPositionProvider => new PositionProviderBottom(revitRepository),
            PositionProviderType.UpPositionProvider => new PositionProviderUp(revitRepository),
            _ => new PositionProviderCenter(revitRepository)
        };
    }

    public ISphereProvider GetSphereProvider(RevitRepository revitRepository) {
        return new SphereProvider(revitRepository);
    }
}
