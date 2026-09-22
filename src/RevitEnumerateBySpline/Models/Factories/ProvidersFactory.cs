using RevitEnumerateBySpline.Models.Enums;
using RevitEnumerateBySpline.Models.Interfaces;
using RevitEnumerateBySpline.Models.Providers;

namespace RevitEnumerateBySpline.Models.Factories;

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
