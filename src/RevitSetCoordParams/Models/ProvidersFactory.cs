using Autodesk.Revit.DB;

using RevitSetCoordParams.Models.Enums;
using RevitSetCoordParams.Models.Interfaces;
using RevitSetCoordParams.Models.Providers;

namespace RevitSetCoordParams.Models;
internal class ProvidersFactory {
    /// <summary>
    /// Метод получения провайдера элементов по Enum
    /// </summary>
    public IElementsProvider GetElementsProvider(RevitRepository revitRepository, ElementsProviderType type) {
        return type switch {
            ElementsProviderType.AllElementsProvider => new ElementsProviderAll(revitRepository),
            ElementsProviderType.CurrentViewProvider => new ElementsProviderCurrentView(revitRepository),
            ElementsProviderType.SelectedElementsProvider => new ElementsProviderSelected(revitRepository),
            _ => new ElementsProviderAll(revitRepository)
        };
    }
    /// <summary>
    /// Метод получения провайдера документа по Enum
    /// </summary>
    public IFileProvider GetFileProvider(RevitRepository revitRepository, Document document) {
        return new FileProvider(revitRepository, document);
    }
    /// <summary>
    /// Метод получения провайдера позиции по Enum
    /// </summary>
    public IPositionProvider GetPositionProvider(RevitRepository revitRepository, PositionProviderType type) {
        return type switch {
            PositionProviderType.CenterPositionProvider => new PositionProviderCenter(revitRepository),
            PositionProviderType.BottomPositionProvider => new PositionProviderBottom(revitRepository),
            PositionProviderType.UpPositionProvider => new PositionProviderUp(revitRepository),
            _ => new PositionProviderCenter(revitRepository)
        };
    }
}
