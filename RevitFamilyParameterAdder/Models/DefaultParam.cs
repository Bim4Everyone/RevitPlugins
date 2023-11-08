using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitFamilyParameterAdder.Models {
    internal class DefaultParam {

#if REVIT_2023_OR_LESS
        public DefaultParam(string name, BuiltInParameterGroup builtInParameterGroup) {
            ParamName = name;
            BINParameterGroup = new ParameterGroupHelper(builtInParameterGroup);
        }

        public DefaultParam(string name, BuiltInParameterGroup builtInParameterGroup, string formula) {
            ParamName = name;
            BINParameterGroup = new ParameterGroupHelper(builtInParameterGroup);
            Formula = formula;
        }
#else
        public DefaultParam(string name, ForgeTypeId builtInParameterGroup) {
            ParamName = name;
            BINParameterGroup = new ParameterGroupHelper(builtInParameterGroup);
        }

        public DefaultParam(string name, ForgeTypeId builtInParameterGroup, string formula) {
            ParamName = name;
            BINParameterGroup = new ParameterGroupHelper(builtInParameterGroup);
            Formula = formula;
        }
#endif

        public string ParamName { get; set; }
        public ParameterGroupHelper BINParameterGroup { get; set; }
        public string Formula { get; set; } = string.Empty;
    }
}
