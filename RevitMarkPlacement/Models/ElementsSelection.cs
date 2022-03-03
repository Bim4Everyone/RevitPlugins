using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitMarkPlacement.Models {

    internal class ElementsSelection : ISelectionMode {
        public IEnumerable<SpotDimension> GetSpotDimentions(Document doc) {
            return new UIDocument(doc).Selection.GetElementIds().Any() ?
                new FilteredElementCollector(doc, new UIDocument(doc).Selection.GetElementIds())
                .OfClass(typeof(SpotDimension))
                .Cast<SpotDimension>() : new List<SpotDimension>();
        }
    }
}
