using System.Collections.Generic;
using System.Linq;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitSetCoordParams.Models.Enums;
using RevitSetCoordParams.Models.Settings;

namespace RevitSetCoordParams.Models;
internal class ParamManager {
    private readonly SetCoordParamsSettings _settings;
    private readonly List<ParamMap> _paramMaps;

    public ParamManager(SetCoordParamsSettings settings) {
        _settings = settings;
        _paramMaps = _settings.ParamMaps;
    }

    public bool BlockingCheck(RevitElement targetElement) {
        var blockingParamMap = _paramMaps
            .FirstOrDefault(paramMap => paramMap.Type == ParamType.BlockingParam);

        if(blockingParamMap is null) {
            return false;
        }

        string targetParamNane = blockingParamMap.TargetParam.Name;
        if(targetElement.Element.IsExistsParam(targetParamNane)) {
            int blockValue = targetElement.Element.GetParamValueOrDefault<int>(targetParamNane);
            if(blockValue is not default(int) and 1) {
                return true;
            }
        }
        return false;
    }

    public List<RevitParam> SetParams(RevitElement sourceElement, RevitElement targetElement) {
        var pairParamMaps = _paramMaps
            .Where(x => x.Type != ParamType.BlockingParam);

        var missedParams = new List<RevitParam>();

        foreach(var paramMap in pairParamMaps) {
            string sourceParamName = paramMap.SourceParam.Name;
            string targetParamName = paramMap.TargetParam.Name;

            if(!targetElement.Element.IsExistsParam(targetParamName) || !sourceElement.Element.IsExistsParam(sourceParamName)) {
                missedParams.Add(paramMap.TargetParam);
                continue;
            }

            switch(paramMap.Type) {
                case ParamType.FloorDEParam:
                    SetDoubleParam(sourceElement, targetElement, sourceParamName, targetParamName);
                    break;

                default:
                    SetStringParam(sourceElement, targetElement, sourceParamName, targetParamName);
                    break;
            }
        }
        return missedParams;
    }

    private void SetDoubleParam(RevitElement sourceElement, RevitElement targetElement, string sourceParamName, string targetParamName) {
        double value = sourceElement.Element.GetParamValueOrDefault<double>(sourceParamName);
        if(value == default) {
            return;
        }
        targetElement.Element.SetParamValue(targetParamName, value);
    }

    private void SetStringParam(RevitElement sourceElement, RevitElement targetElement, string sourceParamName, string targetParamName) {
        string value = sourceElement.Element.GetParamValueOrDefault<string>(sourceParamName);

        if(string.IsNullOrEmpty(value)) {
            return;
        }
        targetElement.Element.SetParamValue(targetParamName, value);
    }
}
