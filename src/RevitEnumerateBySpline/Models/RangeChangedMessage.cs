using RevitEnumerateBySpline.Models.Interfaces;

namespace RevitEnumerateBySpline.Models;

/// <summary>Сообщение об изменении диапазона помещений.</summary>
internal sealed class RangeChangedMessage(IElementsProvider? provider) {
    public IElementsProvider? Provider { get; } = provider;
}
