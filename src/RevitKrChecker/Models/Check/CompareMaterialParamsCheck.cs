// Ignore Spelling: Tooltip

using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitKrChecker.Models.Rule;
using RevitKrChecker.Models.Services;

namespace RevitKrChecker.Models.Check {
    public class CompareMaterialParamsCheck : ICheck {
        private readonly ParamService _paramService;

        public CompareMaterialParamsCheck(string checkName,
                          string targetParamName,
                          ICheckRule checkRule,
                          string sourceParamName,
                          ParamLevel sourceParamLevel) {


            CheckName = checkName ?? throw new ArgumentNullException(nameof(checkName));
            TargetParamName = targetParamName ?? throw new ArgumentNullException(nameof(targetParamName));
            CheckRule = checkRule ?? throw new ArgumentNullException(nameof(checkRule));
            SourceParamName = sourceParamName ?? throw new ArgumentNullException(nameof(sourceParamName));
            SourceParamLevel = sourceParamLevel;

            _paramService = new ParamService();
        }

        public string CheckName { get; }
        public string TargetParamName { get; }
        public ICheckRule CheckRule { get; }
        private string SourceParamName { get; }
        public ParamLevel SourceParamLevel { get; }


        public bool Check(Element element, out CheckInfo info) {
            Document doc = element.Document;

            // Если сравнивать будем не с параметром материала, а с параметров экземпляра или типа,
            // то значение будет одно и то же для разных материалов. Получать его потом много раз в цикле нет смысла
            // В этом случае получим значение, с которым будем сравнивать сразу
            string sourceParamValue = string.Empty;
            if(SourceParamLevel != ParamLevel.Material) {
                Parameter sourceParam = _paramService.GetParamToCheck(element, SourceParamName, SourceParamLevel);
                sourceParamValue = sourceParam.AsValueString();
            }

            List<Element> materials = element.GetMaterialIds(false)
                           .Select(id => doc.GetElement(id))
                           .ToList();

            foreach(Element material in materials) {
                string targetParamValue = material.GetParamValue<string>(TargetParamName);

                // Если сравнивать будем со значением параметра на уровне материала,
                // то будем получать его значение каждый раз в рамках материала
                if(SourceParamLevel is ParamLevel.Material) {
                    sourceParamValue = material.GetParamValue<string>(SourceParamName);
                }

                if(!CheckRule.Check(targetParamValue, sourceParamValue)) {
                    info = new CheckInfo(CheckName, TargetParamName, element, GetTooltip());
                    return false;
                }
            }
            info = null;
            return true;
        }


        public string GetTooltip() {
            return $"{CheckName}: значение параметра \"{TargetParamName}\" {CheckRule.UnfulfilledRule} \"{SourceParamName}\"";
        }
    }
}
