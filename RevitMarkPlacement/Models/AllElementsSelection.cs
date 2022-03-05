using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Models {
    internal class AllElementsSelection : ISelectionMode {
        public IEnumerable<SpotDimension> GetSpotDimentions(Document doc) {
            return new FilteredElementCollector(doc)
            .OfClass(typeof(SpotDimension))
            .OfType<SpotDimension>();
        }
    }
}
