using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitMarkingElements.Models {
    internal class MarkingContext {
        public int Counter { get; set; }
        public List<Element> ProcessedElements { get; } = new List<Element>();
        public List<Element> MarkingElements { get; set; }
        public List<CurveElement> Lines { get; set; }
    }
}
