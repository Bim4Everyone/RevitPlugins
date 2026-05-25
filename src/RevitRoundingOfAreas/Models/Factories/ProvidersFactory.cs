using RevitRoundingOfAreas.Models.Enums;
using RevitRoundingOfAreas.Models.Interfaces;
using RevitRoundingOfAreas.Models.Providers;

namespace RevitRoundingOfAreas.Models.Factories;

internal class ProvidersFactory {

    private readonly RevitRepository _revitRepository;

    public ProvidersFactory(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }

    /// <summary>
    /// Метод получения провайдера элементов по Enum
    /// </summary>
    public IElementsProvider GetElementsProvider(ElementsProviderType type) {
        return type switch {
            ElementsProviderType.AllElementsProvider => new ElementsProviderAll(_revitRepository),
            ElementsProviderType.CurrentViewProvider => new ElementsProviderCurrentView(_revitRepository),
            ElementsProviderType.SelectedElementsProvider => new ElementsProviderSelected(_revitRepository),
            _ => new ElementsProviderAll(_revitRepository)
        };
    }
}
