using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.FilterCreators.RevitFilterCreators {
    internal class RevitLogicalAndFilterCreator : IRevitLogicalFilterCreator {
        public ElementLogicalFilter Create(IList<ElementFilter> elementFilters) {
            return new LogicalAndFilter(elementFilters);
        }
    }
    internal class RevitLogicalOrFilterCreator : IRevitLogicalFilterCreator {
        public ElementLogicalFilter Create(IList<ElementFilter> elementFilters) {
            return new LogicalOrFilter(elementFilters);
        }
    }
}
