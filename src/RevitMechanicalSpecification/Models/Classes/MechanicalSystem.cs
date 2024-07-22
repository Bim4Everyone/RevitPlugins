using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitMechanicalSpecification.Models.Classes {
    public class MechanicalSystem {
        public MechanicalSystem() { }
        public string SystemName { get; set; }
        public string SystemFunction { get; set; }
        public string SystemShortName { get; set; }
        public MEPSystem SystemElement { get; set; }

    }
}
