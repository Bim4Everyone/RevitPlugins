using System.Collections.Generic;

using RevitSplitMepCurve.Models.Enums;
using RevitSplitMepCurve.Models.Splittable;

namespace RevitSplitMepCurve.Services.Providers;

internal interface IElementsProvider {
    MepClass MepClass { get; }

    ICollection<SplittableElement> GetElements(SelectionMode selectionMode);
}
