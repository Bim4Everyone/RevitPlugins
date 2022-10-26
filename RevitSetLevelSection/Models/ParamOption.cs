using dosymep.Bim4Everyone;
using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitSetLevelSection.Models {
    internal class ParamOption {
        public string ProjectRevitParamName { get; set; }
        public RevitParam SharedRevitParam { get; set; }
    }

    internal static class ElementExtensions {
        public static T GetParamValue<T>(this Element element, ParamOption paramOption) {
            if(element.IsExistsProjectParam(paramOption.ProjectRevitParamName)) {
                return element.GetParamValue<T>(paramOption.ProjectRevitParamName);
            } else {
                return element.GetParamValue<T>(paramOption.SharedRevitParam);
            }
        }
    }
}