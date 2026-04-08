using RevitSuperfilter.Models;

namespace RevitSuperfilter.Services;

internal interface ISuperfilterService : IElementIndex {
    void Build(ISelectionElements selectionElements);
}
