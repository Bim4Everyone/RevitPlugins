using dosymep.Bim4Everyone;
using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

namespace RevitSetLevelSection.Models {
    internal class ParamOption {
        public static readonly string AdskLevelName = "ADSK_Этаж";
        public static readonly string AdskSectionNumberName = "ADSK_Номер секции";
        public static readonly string AdskBuildingNumberName = "ADSK_Номер здания";
        
        public static readonly string BuildingWorksBlockName = "Блок СМР_";
        public static readonly string BuildingWorksSectionName = "Секция СМР_";
        public static readonly string BuildingWorksTypingName = "Типизация СМР_";

        public static readonly ParamOption BuildingWorksBlock = new ParamOption() {
            IsRequired = true,
            RevitParamName = BuildingWorksBlockName,
            RevitParam = SharedParamsConfig.Instance.BuildingWorksBlock
        };

        public static readonly ParamOption BuildingWorksSection = new ParamOption() {
            RevitParamName = BuildingWorksSectionName,
            RevitParam = SharedParamsConfig.Instance.BuildingWorksSection
        };
        
        public static readonly ParamOption BuildingWorksTyping = new ParamOption() {
            RevitParamName = BuildingWorksTypingName,
            RevitParam = SharedParamsConfig.Instance.BuildingWorksTyping
        };

        public bool IsRequired { get; set; }
        public string RevitParamName { get; set; }
        public RevitParam RevitParam { get; set; }
    }

    internal static class ElementExtensions {
        public static bool IsExistsParamValue(this Element element, ParamOption paramOption) {
            if(element.IsExistsProjectParamValue(paramOption.RevitParamName)) {
                return true;
            } else {
                return element.IsExistsParamValue(paramOption.RevitParam);
            }
        }
        
        public static T GetParamValue<T>(this Element element, ParamOption paramOption) {
            if(element.IsExistsProjectParamValue(paramOption.RevitParamName)) {
                return element.GetParamValue<T>(paramOption.RevitParamName);
            } else {
                return element.GetParamValue<T>(paramOption.RevitParam);
            }
        }
    }
}