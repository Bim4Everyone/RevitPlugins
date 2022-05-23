using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitClashDetective.Models {
    internal class Collector {
        public Collector(Document doc) {
            Document = doc;
            RevitCollector = new FilteredElementCollector(doc);
        }
        public Document Document { get; }
        public FilteredElementCollector RevitCollector { get; }
    }
}
