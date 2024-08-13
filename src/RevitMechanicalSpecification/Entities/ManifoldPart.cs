using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitMechanicalSpecification.Entities {
    public class ManifoldPart {
        public ElementId Id { get; set; }
        public string Group { get; set; }
    }
}
