using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitKrChecker.Models.CheckOptions;
using RevitKrChecker.Models.Interfaces;
using RevitKrChecker.Models.Services;

namespace RevitKrChecker.Models.Check;
public class CompareMaterialParamsCheck : ICheck {
    private readonly ILocalizationService _localizationService;
    private readonly ParamValueService _paramService;

    public CompareMaterialParamsCheck(
        CompareCheckOptions checkOptions,
        ILocalizationService localizationService,
        ParamValueService paramValueService) {

        _localizationService = localizationService;
        _paramService = paramValueService;

        CheckName = checkOptions.CheckName
            ?? throw new ArgumentNullException(nameof(checkOptions.CheckName));

        TargetParamName = checkOptions.TargetParamName
            ?? throw new ArgumentNullException(nameof(checkOptions.TargetParamName));
        // Проверяем, что параметр для проверке на уровне материала
        TargetParamLevel = checkOptions.TargetParamLevel != ParamLevel.Material
            ? throw new ArgumentException(
                _localizationService.GetLocalizedString("ReportWindow.CheckNotForElementParameter"))
            : checkOptions.TargetParamLevel;

        CheckRule = checkOptions.CheckRule
            ?? throw new ArgumentNullException(nameof(checkOptions.CheckRule));

        SourceParamName = checkOptions.SourceParamName
            ?? throw new ArgumentNullException(nameof(checkOptions.SourceParamName));
        SourceParamLevel = checkOptions.SourceParamLevel != ParamLevel.Material
            ? throw new ArgumentException(
                _localizationService.GetLocalizedString("ReportWindow.CheckNotForCompareWithElementParameter"))
            : checkOptions.SourceParamLevel;
    }

    public string CheckName { get; }
    public string TargetParamName { get; }
    public ParamLevel TargetParamLevel { get; }
    public ICheckRule CheckRule { get; }
    private string SourceParamName { get; }
    public ParamLevel SourceParamLevel { get; }

    public bool Check(Element element, out CheckInfo info) {
        if(element == null) {
            throw new ArgumentNullException(nameof(element));
        }

        var doc = element.Document;
        var materials = element.GetMaterialIds(false)
                       .Select(doc.GetElement)
                       .ToList();

        foreach(var material in materials) {
            string targetParamValue = material.GetParam(TargetParamName).AsValueString();
            string sourceParamValue = material.GetParam(SourceParamName).AsValueString();

            if(!CheckRule.Check(targetParamValue, sourceParamValue)) {
                info = new CheckInfo(CheckName, TargetParamName, element, GetTooltip());
                return false;
            }
        }
        info = null;
        return true;
    }

    public string GetTooltip() {
        // "значение параметра"
        string parameterValue = _localizationService.GetLocalizedString("ReportWindow.ParameterValue");
        return $"{CheckName}: {parameterValue} \"{TargetParamName}\" " +
            $"{CheckRule.UnfulfilledRule} \"{SourceParamName}\"";
    }
}
