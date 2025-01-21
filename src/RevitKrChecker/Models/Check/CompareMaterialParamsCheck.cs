using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitKrChecker.Models.CheckOptions;
using RevitKrChecker.Models.Interfaces;
using RevitKrChecker.Models.Services;

namespace RevitKrChecker.Models.Check {
    public class CompareMaterialParamsCheck : ICheck {
        private readonly ParamService _paramService;

        public CompareMaterialParamsCheck(CompareCheckOptions checkOptions) {
            CheckName = checkOptions.CheckName
                ?? throw new ArgumentNullException(nameof(checkOptions.CheckName));
            TargetParamName = checkOptions.TargetParamName
                ?? throw new ArgumentNullException(nameof(checkOptions.TargetParamName));
            // Проверяем, что параметр для проверке на уровне материала
            if(checkOptions.TargetParamLevel != ParamLevel.Material)
                throw new ArgumentException("Проверка предусмотрена только для проверки параметра материала");

            CheckRule = checkOptions.CheckRule
                ?? throw new ArgumentNullException(nameof(checkOptions.CheckRule));
            SourceParamName = checkOptions.SourceParamName
                ?? throw new ArgumentNullException(nameof(checkOptions.SourceParamName));
            SourceParamLevel = checkOptions.SourceParamLevel;

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
                string targetParamValue = material.GetParam(TargetParamName).AsValueString();

                // Если сравнивать будем со значением параметра на уровне материала,
                // то будем получать его значение каждый раз в рамках материала
                if(SourceParamLevel is ParamLevel.Material) {
                    sourceParamValue = material.GetParam(SourceParamName).AsValueString();
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
