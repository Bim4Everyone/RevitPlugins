using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitBatchPrint.Extensions;

internal static class ElementExtensions {
    public static string GetParamDisplayValue(this Element element, RevitParam revitParam) {
        if(revitParam is null) {
            return null;
        }
        
        if(!element.IsExistsParam(revitParam)) {
            return null;
        }

        var parameter = element.GetParam(revitParam);
        string paramValue = parameter.AsValueString();

        if(string.IsNullOrEmpty(paramValue)) {
            return element.GetParamValueOrDefault(revitParam)?.ToString();
        }

        return paramValue;
    }
}
