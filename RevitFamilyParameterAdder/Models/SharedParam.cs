using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitFamilyParameterAdder.Models
{
    internal class SharedParam
    {
        //public SharedParam() {

        //}
        public SharedParam(ExternalDefinition externalDefinition, List<ParameterGroupHelper> bINParameterGroups) {
            ParamName = externalDefinition.Name;
            ParamInShPF = externalDefinition;
            ParamGroupInShPF = externalDefinition.OwnerGroup.Name;
            ParamGroupsInFM = bINParameterGroups;
        }

        public string ParamName { get; set; }
        public ExternalDefinition ParamInShPF { get; set; }
        public string ParamGroupInShPF { get; set; }


        public List<ParameterGroupHelper> ParamGroupsInFM { get; set; }
        public ParameterGroupHelper SelectedParamGroupInFM { get; set; }
    }
}
