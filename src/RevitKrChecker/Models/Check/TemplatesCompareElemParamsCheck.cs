using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitKrChecker.Models.CheckOptions;
using RevitKrChecker.Models.Interfaces;
using RevitKrChecker.Models.Services;

namespace RevitKrChecker.Models.Check;
public class TemplatesCompareElemParamsCheck : ICheck {
    private readonly ILocalizationService _localizationService;
    private readonly ParamValueService _paramService;

    public TemplatesCompareElemParamsCheck(
        TemplatesCompareCheckOptions checkOptions,
        ILocalizationService localizationService,
        ParamValueService paramValueService) {

        _localizationService = localizationService;
        _paramService = paramValueService;

        CheckName = checkOptions.CheckName
            ?? throw new ArgumentNullException(nameof(checkOptions.CheckName));

        TargetParamName = checkOptions.TargetParamName
            ?? throw new ArgumentNullException(nameof(checkOptions.TargetParamName));
        // Проверяем, что параметр для проверке не на уровне материала
        TargetParamLevel = checkOptions.TargetParamLevel is ParamLevel.Material
            ? throw new ArgumentException(
                _localizationService.GetLocalizedString("ReportWindow.CheckNotForMaterialParameter"))
            : checkOptions.TargetParamLevel;

        CheckRule = checkOptions.CheckRule
            ?? throw new ArgumentNullException(nameof(checkOptions.CheckRule));

        SourceParamName = checkOptions.SourceParamName
            ?? throw new ArgumentNullException(nameof(checkOptions.SourceParamName));
        SourceParamLevel = checkOptions.SourceParamLevel;

        DictForCompare = checkOptions.DictForCompare
            ?? throw new ArgumentNullException(nameof(checkOptions.DictForCompare));
        DictForCompareRule = checkOptions.DictForCompareRule;
    }

    public string CheckName { get; }
    public string TargetParamName { get; }
    public ParamLevel TargetParamLevel { get; }
    public ICheckRule CheckRule { get; }
    private string SourceParamName { get; }
    public ParamLevel SourceParamLevel { get; }
    private Dictionary<string, string> DictForCompare { get; }
    public ICheckRule DictForCompareRule { get; }


    private bool CheckAllValues(string targetParamValue, List<string> sourceParamValues) {
        return sourceParamValues.All(sourceParamValue => CheckRule.Check(targetParamValue, sourceParamValue));
    }

    public bool Check(Element element, out CheckInfo info) {
        if(element == null) {
            throw new ArgumentNullException(nameof(element));
        }

        string targetParamValue = _paramService.GetParamValueToCheck(element, TargetParamName, TargetParamLevel);
        List<string> sourceParamValues =
            _paramService.GetParamValuesToCheck(element, SourceParamName, SourceParamLevel);
        List<string> sourceParamValuesByDict = sourceParamValues
            .Select(val => _paramService.GetValueByDict(DictForCompare, DictForCompareRule, val))
            .ToList();

        if(!CheckAllValues(targetParamValue, sourceParamValuesByDict)) {
            info = new CheckInfo(CheckName, TargetParamName, element, GetTooltip());
            return false;
        }
        info = null;
        return true;
    }


    private string GetDictForCompareAsStr() {
        string separator = ", ";

        List<string> pairs = DictForCompare.Keys
            .Select(key => $"\"{key}\": \"{DictForCompare[key]}\"")
            .ToList();
        return string.Join(separator, pairs);
    }

    public string GetTooltip() {
        // "значение параметра"
        string parameterValue = _localizationService.GetLocalizedString("ReportWindow.ParameterValue");
        // "по правилу"
        string byRule = _localizationService.GetLocalizedString("ReportWindow.ByRule");
        return $"{CheckName}: {parameterValue} \"{TargetParamName}\" {CheckRule.UnfulfilledRule}" +
            $" \"{SourceParamName}\" {byRule}: {GetDictForCompareAsStr()}";
    }
}
