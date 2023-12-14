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

#if REVIT_2023_OR_LESS
        public ParameterGroupHelper(BuiltInParameterGroup builtInParameterGroup) {
            BuiltInParamGroup = builtInParameterGroup;
            GroupName = LabelUtils.GetLabelFor(builtInParameterGroup);
        }
#else
        public ParameterGroupHelper(ForgeTypeId builtInParameterGroup) {
            BuiltInParamGroup = builtInParameterGroup;
            if(builtInParameterGroup != null) {
                GroupName = LabelUtils.GetLabelForGroup(builtInParameterGroup);
            }
        }
#endif

        internal object BuiltInParamGroup { get; set; }
        public string GroupName { get; set; }
    }
}
