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
        public ParameterGroupHelper(object builtInParameterGroup) {
            BuiltInParamGroup = builtInParameterGroup;
#if REVIT_2023_OR_LESS
            GroupName = LabelUtils.GetLabelFor((BuiltInParameterGroup) builtInParameterGroup);
#else
            GroupName = LabelUtils.GetLabelForGroup((ForgeTypeId) builtInParameterGroup);
#endif
        }

        internal object BuiltInParamGroup { get; set; }
        public string GroupName { get; set; }
    }
}
