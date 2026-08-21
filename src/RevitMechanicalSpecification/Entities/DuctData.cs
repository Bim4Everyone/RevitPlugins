using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;

namespace RevitMechanicalSpecification.Entities {
    internal class DuctData {
        public Duct Duct { get; set; }
        public ElementId OldDuctId { get; set; }
        public ElementId DuctTypeId { get; set; }
        public ElementId SystemTypeId { get; set; }
        public ElementId LevelId { get; set; }
        public XYZ StartPoint { get; set; }
        public XYZ EndPoint { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsPinned { get; set; }
        public List<ElementParameterValue> Parameters { get; set; }
        public List<DuctEnd> Ends { get; set; }
        public List<InsulationData> Insulations { get; set; }
    }
}
