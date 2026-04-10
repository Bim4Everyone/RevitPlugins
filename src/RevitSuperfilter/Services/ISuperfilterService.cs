using RevitSuperfilter.Models;

namespace RevitSuperfilter.Services;

internal interface ISuperfilterService : IElementIndex {
    string Selection { get; }
    Superfilter Superfilter { get; }
    ElementsIndex ElementsIndex { get; }

    void Build();
}
