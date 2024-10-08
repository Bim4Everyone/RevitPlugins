using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitMechanicalSpecification.Entities {
    public class VisSystem {
        public string SystemSystemName { get; set; }
        public string SystemFunction { get; set; }
        public string SystemShortName { get; set; }
        public string SystemTargetName { get; set; }
        public MEPSystem SystemElement { get; set; }
    }
}
