using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitCreatingFiltersByValues.Models {
    internal class ParametersHelper {
        public ParametersHelper() {}


        public string ParamName { get; set; }
        public BuiltInParameter BInParameter { get; set; }
        public ParameterElement ParamElement { get; set; }

        public bool IsBInParam { get; set; } = false;


        public ElementId Id { get; set; }
    }
}
