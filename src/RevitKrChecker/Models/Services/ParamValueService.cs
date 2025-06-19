using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitKrChecker.Models.Interfaces;

namespace RevitKrChecker.Models.Services;
public class ParamValueService {
    private readonly ILocalizationService _localizationService;

    public ParamValueService(ILocalizationService localizationService) {
        _localizationService = localizationService;
    }

    public string GetParamValueToCheck(Element element, string paramName, ParamLevel paramLevel) {
        var doc = element.Document;
        return paramLevel switch {
            ParamLevel.Instance => element.GetParam(paramName).AsValueString(),
            ParamLevel.Type => doc.GetElement(element.GetTypeId()).GetParam(paramName).AsValueString(),
            ParamLevel.Material => element
                                    .GetMaterialIds(false)
                                    .Select(doc.GetElement)
                                    .Select(material => material.GetParam(paramName).AsValueString())
                                    .FirstOrDefault(),
            _ => throw new ArgumentOutOfRangeException(
                                    nameof(paramLevel),
                                    _localizationService.GetLocalizedString("ReportWindow.SetUnknownParameterLevel")),
        };
    }

    public List<string> GetParamValuesToCheck(Element element, string paramName, ParamLevel paramLevel) {
        var doc = element.Document;
        return paramLevel switch {
            ParamLevel.Instance => [
                                    element.GetParam(paramName).AsValueString()
                                ],
            ParamLevel.Type => [
                                    doc.GetElement(element.GetTypeId()).GetParam(paramName).AsValueString()
                                ],
            ParamLevel.Material => element
                                    .GetMaterialIds(false)
                                    .Select(doc.GetElement)
                                    .Select(material => material.GetParam(paramName).AsValueString())
                                    .ToList(),
            _ => throw new ArgumentOutOfRangeException(
                                    nameof(paramLevel),
                                    _localizationService.GetLocalizedString("ReportWindow.SetUnknownParameterLevel")),
        };
    }

    private string GetDictForCompareKeyByRule(Dictionary<string, string> dictForCompare,
                                              ICheckRule dictForCompareRule, string value) {
        return dictForCompare.Keys.FirstOrDefault(key => dictForCompareRule.Check(value, key));
    }

    public string GetValueByDict(Dictionary<string, string> dictForCompare, ICheckRule dictForCompareRule,
                                 string value) {
        string key = GetDictForCompareKeyByRule(dictForCompare, dictForCompareRule, value);
        return key is null ? null : dictForCompare[key];
    }
}
