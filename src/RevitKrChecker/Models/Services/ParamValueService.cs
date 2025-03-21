using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitKrChecker.Models.Interfaces;

namespace RevitKrChecker.Models.Services {
    public class ParamValueService {
        private readonly ILocalizationService _localizationService;

        public ParamValueService(ILocalizationService localizationService) {
            _localizationService = localizationService;
        }

        public string GetParamValueToCheck(Element element, string paramName, ParamLevel paramLevel) {
            Document doc = element.Document;
            switch(paramLevel) {
                case ParamLevel.Instance:
                    return element.GetParam(paramName).AsValueString();
                case ParamLevel.Type:
                    return doc.GetElement(element.GetTypeId()).GetParam(paramName).AsValueString();
                case ParamLevel.Material:
                    return element
                        .GetMaterialIds(false)
                        .Select(id => doc.GetElement(id))
                        .Select(material => material.GetParam(paramName).AsValueString())
                        .FirstOrDefault();
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(paramLevel),
                        _localizationService.GetLocalizedString("ReportWindow.SetUnknownParameterLevel"));
            }
        }

        public List<string> GetParamValuesToCheck(Element element, string paramName, ParamLevel paramLevel) {
            Document doc = element.Document;
            switch(paramLevel) {
                case ParamLevel.Instance:
                    return new List<string> {
                        element.GetParam(paramName).AsValueString()
                    };
                case ParamLevel.Type:
                    return new List<string> {
                        doc.GetElement(element.GetTypeId()).GetParam(paramName).AsValueString()
                    };
                case ParamLevel.Material:
                    return element
                        .GetMaterialIds(false)
                        .Select(id => doc.GetElement(id))
                        .Select(material => material.GetParam(paramName).AsValueString())
                        .ToList();
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(paramLevel),
                        _localizationService.GetLocalizedString("ReportWindow.SetUnknownParameterLevel"));
            }
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
}
