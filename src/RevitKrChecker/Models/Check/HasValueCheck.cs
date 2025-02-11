using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitKrChecker.Models.CheckOptions;
using RevitKrChecker.Models.Interfaces;
using RevitKrChecker.Models.Services;

namespace RevitKrChecker.Models.Check {
    public class HasValueCheck : ICheck {
        private readonly ILocalizationService _localizationService;
        private readonly ParamValueService _paramService;

        public HasValueCheck(
            HasValueCheckOptions checkOptions,
            ILocalizationService localizationService,
            ParamValueService paramValueService) {

            _localizationService = localizationService;
            _paramService = paramValueService;

            CheckName = checkOptions.CheckName ?? throw new ArgumentNullException(nameof(checkOptions.CheckName));

            TargetParamName = checkOptions.TargetParamName
                ?? throw new ArgumentNullException(nameof(checkOptions.TargetParamName));
            TargetParamLevel = checkOptions.TargetParamLevel;
        }


        public string CheckName { get; }
        public string TargetParamName { get; }
        public ParamLevel TargetParamLevel { get; }


        public bool Check(Element element, out CheckInfo info) {
            if(element == null) {
                throw new ArgumentNullException(nameof(element));
            }

            List<string> targetParamValues =
                _paramService.GetParamValuesToCheck(element, TargetParamName, TargetParamLevel);

            foreach(var targetParamValue in targetParamValues) {
                if(string.IsNullOrEmpty(targetParamValue)) {
                    info = new CheckInfo(CheckName, TargetParamName, element, GetTooltip());
                    return false;
                }
            }
            info = null;
            return true;
        }

        public string GetTooltip() {
            // "значение параметра"
            var parameterValue = _localizationService.GetLocalizedString("ReportWindow.ParameterValue");
            // "отсутствует"
            var missing = _localizationService.GetLocalizedString("ReportWindow.Missing");
            return $"{CheckName}: {parameterValue} \"{TargetParamName}\" {missing}";
        }
    }
}
