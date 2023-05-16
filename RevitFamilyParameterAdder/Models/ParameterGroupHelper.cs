using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitFamilyParameterAdder.Models
{
    internal class ParameterGroupHelper
    {
        public ParameterGroupHelper(BuiltInParameterGroup builtInParameterGroup) {
            BuiltInParamGroup = builtInParameterGroup;
            GroupName = LabelUtils.GetLabelFor(builtInParameterGroup);
        }

        internal BuiltInParameterGroup BuiltInParamGroup { get; set; }
        public string GroupName { get; set; }
    }
}
