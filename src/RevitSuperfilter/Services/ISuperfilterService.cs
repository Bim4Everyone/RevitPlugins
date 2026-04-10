using RevitSuperfilter.Models;

namespace RevitSuperfilter.Services;

internal interface ISuperfilterService : IElementIndex {
    Selection Selection { get; }
    string DisplaySelection { get; }
    
    Superfilter Superfilter { get; }
    ElementsIndex ElementsIndex { get; }

    ISuperfilterService Build();
}
