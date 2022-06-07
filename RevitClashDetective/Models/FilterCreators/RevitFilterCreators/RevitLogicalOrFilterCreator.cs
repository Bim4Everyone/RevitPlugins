using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.FilterCreators.RevitFilterCreators {
    internal class RevitLogicalOrFilterCreator : IRevitLogicalFilterCreator {
        public ElementLogicalFilter Create(IList<ElementFilter> elementFilters) {
            return new LogicalOrFilter(elementFilters);
        }
    }
}
