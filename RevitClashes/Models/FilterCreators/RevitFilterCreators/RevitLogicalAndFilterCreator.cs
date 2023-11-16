using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.FilterCreators.RevitFilterCreators {
    internal class RevitLogicalAndFilterCreator : IRevitLogicalFilterCreator {
        public ElementLogicalFilter Create(IList<ElementFilter> elementFilters) {
            return new LogicalAndFilter(elementFilters);
        }
    }
}
