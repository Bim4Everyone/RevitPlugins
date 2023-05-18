using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitFamilyParameterAdder.Models {
    internal class DefaultParam {
        public DefaultParam(string name, BuiltInParameterGroup builtInParameterGroup) {
            ParamName = name;
            BINParameterGroup = new ParameterGroupHelper(builtInParameterGroup);
        }

        public DefaultParam(string name, BuiltInParameterGroup builtInParameterGroup, string formula) {
            ParamName = name;
            BINParameterGroup = new ParameterGroupHelper(builtInParameterGroup);
            Formula = formula;
        }

        public string ParamName { get; set; }
        public ParameterGroupHelper BINParameterGroup { get; set; }
        public string Formula { get; set; } = string.Empty;
    }
}
