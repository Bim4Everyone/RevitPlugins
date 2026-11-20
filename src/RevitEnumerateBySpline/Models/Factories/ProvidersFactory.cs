using RevitEnumerateBySpline.Models.Enums;
using RevitEnumerateBySpline.Models.Interfaces;
using RevitEnumerateBySpline.Models.Providers;

namespace RevitEnumerateBySpline.Models.Factories;

/// <summary>Создаёт источники помещений по выбранному диапазону.</summary>
internal class ProvidersFactory(RevitRepository revitRepository) {
    /// <summary>
    /// Метод получения провайдера элементов по Enum
    /// </summary>
    /// <param name="type">Тип источника помещений.</param>
    /// <returns>Источник помещений выбранного типа.</returns>
    public IElementsProvider GetElementsProvider(ElementsProviderType type) {
        return type switch {
            ElementsProviderType.AllElementsProvider => new ElementsProviderAll(revitRepository),
            ElementsProviderType.CurrentViewProvider => new ElementsProviderCurrentView(revitRepository),
            ElementsProviderType.SelectedElementsProvider => new ElementsProviderSelected(revitRepository),
            _ => new ElementsProviderAll(revitRepository)
        };
    }
}
