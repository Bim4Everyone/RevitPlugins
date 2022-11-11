using dosymep.Bim4Everyone;
using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitSetLevelSection.Models {
    internal class ParamOption {
        public string AdskParamName { get; set; }
        public string ProjectRevitParamName { get; set; }
        public RevitParam SharedRevitParam { get; set; }
    }

    internal static class ElementExtensions {
        public static bool IsExistsParamValue(this Element element, ParamOption paramOption) {
            if(element.IsExistsProjectParam(paramOption.ProjectRevitParamName)) {
                return element.IsExistsProjectParamValue(paramOption.ProjectRevitParamName);
            } else {
                return element.IsExistsParamValue(paramOption.SharedRevitParam);
            }
        }
        
        public static T GetParamValue<T>(this Element element, ParamOption paramOption) {
            if(element.IsExistsProjectParamValue(paramOption.ProjectRevitParamName)) {
                return element.GetParamValue<T>(paramOption.ProjectRevitParamName);
            } else {
                return element.GetParamValue<T>(paramOption.SharedRevitParam);
            }
        }
    }
}