using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitFinishingWalls.ViewModels {
    internal interface IElementsContainer {
        IReadOnlyCollection<ElementId> DependentElements { get; }
    }
}
