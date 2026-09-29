using RevitEnumerateBySpline.Models.Interfaces;

namespace RevitEnumerateBySpline.Models;

internal sealed class RangeChangedMessage(IElementsProvider provider) {
    public IElementsProvider Provider { get; } = provider;
}
