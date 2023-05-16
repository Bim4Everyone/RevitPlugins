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
        public SharedParam(ExternalDefinition externalDefinition, List<ParameterGroupHelper> bINParameterGroups) {
            ParamName = externalDefinition.Name;
            ParamInShPF = externalDefinition;
            ParamGroupInShPF = externalDefinition.OwnerGroup.Name;
            ParamGroupsInFM = bINParameterGroups;
        }

        public string ParamName { get; set; }

        /// <summary>
        /// Объект параметра в ФОП
        /// </summary>
        public ExternalDefinition ParamInShPF { get; set; }

        /// <summary>
        /// Группа параметров в ФОП
        /// </summary>
        public string ParamGroupInShPF { get; set; }


        /// <summary>
        /// Список групп параметров в семействе
        /// </summary>
        public List<ParameterGroupHelper> ParamGroupsInFM { get; set; }

        /// <summary>
        /// Выбранная группа для группировки параметра в семействе
        /// </summary>
        public ParameterGroupHelper SelectedParamGroupInFM { get; set; }

        /// <summary>
        /// Уровень размещения параметра - экземпляр/тип
        /// </summary>
        public bool IsInstanceParam { get; set; } = true;
    }
}
