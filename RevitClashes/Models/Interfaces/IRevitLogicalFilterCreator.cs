using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitClashDetective.Models.Interfaces {
    interface IRevitLogicalFilterCreator {
        ElementLogicalFilter Create(IList<ElementFilter> elementFilters);
    }
}
