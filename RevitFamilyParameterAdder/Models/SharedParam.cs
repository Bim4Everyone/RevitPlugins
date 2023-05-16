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
        public SharedParam() {

        }
        public SharedParam(ExternalDefinition externalDefinition) {
            ParamName = externalDefinition.Name;
            ParamInShPF = externalDefinition;
            ParamGroupInShPF = externalDefinition.OwnerGroup.Name;
        }

        public string ParamName { get; set; }
        public ExternalDefinition ParamInShPF { get; set; }
        public string ParamGroupInShPF { get; set; }
        public BuiltInParameterGroup ParamGroupInFamily { get; set; }

    }
}
