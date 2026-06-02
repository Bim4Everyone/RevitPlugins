using RevitRoundingOfAreas.Models.Enums;
using RevitRoundingOfAreas.Models.Interfaces;
using RevitRoundingOfAreas.Models.Providers;

namespace RevitRoundingOfAreas.Models.Factories;

internal class ProvidersFactory(RevitRepository revitRepository) {
    /// <summary>
    /// Метод получения провайдера элементов по Enum
    /// </summary>
    public IElementsProvider GetElementsProvider(ElementsProviderType type) {
        return type switch {
            ElementsProviderType.AllElementsProvider => new ElementsProviderAll(revitRepository),
            ElementsProviderType.CurrentViewProvider => new ElementsProviderCurrentView(revitRepository),
            ElementsProviderType.SelectedElementsProvider => new ElementsProviderSelected(revitRepository),
            _ => new ElementsProviderAll(revitRepository)
        };
    }
}
