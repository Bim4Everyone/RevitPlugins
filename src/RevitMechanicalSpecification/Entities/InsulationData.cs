using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;

namespace RevitMechanicalSpecification.Entities {
    internal class InsulationData {
        public DuctInsulation Element { get; set; }
        public ElementId OldElementId { get; set; }
        public ElementId TypeId { get; set; }
        public double Thickness { get; set; }
        public List<ElementParameterValue> Parameters { get; set; }
    }
}
