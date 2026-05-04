using System.Collections.Generic;
using System.Linq;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.Services;

internal interface ISortElementService {
    IOrderedEnumerable<MarkedElement> SortElements(IReadOnlyList<MarkedElement> elements,
                                                   IReadOnlyList<FilterableParam> sortParams);
}
