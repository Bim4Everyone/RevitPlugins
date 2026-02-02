using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

namespace RevitUnmodelingMep.Models.Unmodeling;

internal sealed class ProjectStockProvider {
    private readonly Document _doc;
    private readonly Dictionary<BuiltInCategory, double> _cache = new();

    public ProjectStockProvider(Document doc) {
        _doc = doc;
    }

    public double Get(BuiltInCategory category) {
        if(_cache.TryGetValue(category, out double cached)) {
            return cached;
        }

        double value;
        if(category == BuiltInCategory.OST_PipeCurves || category == BuiltInCategory.OST_DuctCurves) {
            value = 1 + _doc.ProjectInformation.GetParamValueOrDefault<double>(
                SharedParamsConfig.Instance.VISPipeDuctReserve) / 100;
        } else if(category == BuiltInCategory.OST_PipeInsulations) {
            value = 1 + _doc.ProjectInformation.GetParamValueOrDefault<double>(
                SharedParamsConfig.Instance.VISPipeInsulationReserve) / 100;
        } else if(category == BuiltInCategory.OST_DuctInsulations) {
            value = 1 + _doc.ProjectInformation.GetParamValueOrDefault<double>(
                SharedParamsConfig.Instance.VISDuctInsulationReserve) / 100;
        } else {
            value = 1;
        }

        _cache[category] = value;
        return value;
    }
}
