using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitFamilyParameterAdder.Models {
    internal class BINParameterGroupHelper {
        public BINParameterGroupHelper(BuiltInParameterGroup builtInParameterGroup) {
            BINParameterGroup = builtInParameterGroup;
        }
        public BINParameterGroupHelper(BuiltInParameterGroup builtInParameterGroup, string formula) {
            BINParameterGroup = builtInParameterGroup;
            Formula = formula;
        }

        public BuiltInParameterGroup BINParameterGroup { get; set; }
        public string Formula { get; set; } = string.Empty;
    }
}
