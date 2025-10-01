using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitClashDetective.Models.Interfaces;
internal interface IRevitLogicalFilterCreator {
    ElementLogicalFilter Create(IList<ElementFilter> elementFilters);
}
