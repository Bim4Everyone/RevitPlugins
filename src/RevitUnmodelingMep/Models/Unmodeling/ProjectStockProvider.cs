using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

namespace RevitUnmodelingMep.Models.Unmodeling;

internal sealed class ProjectStockProvider {
    private readonly Document _doc;

    public ProjectStockProvider(Document doc) {
        _doc = doc;
    }

    public double Get(BuiltInCategory category) {
        if(category == BuiltInCategory.OST_PipeCurves || category == BuiltInCategory.OST_DuctCurves) {
            return 1 + _doc.ProjectInformation.GetParamValueOrDefault<double>(
                SharedParamsConfig.Instance.VISPipeDuctReserve) / 100;
        }
        if(category == BuiltInCategory.OST_PipeInsulations) {
            return 1 + _doc.ProjectInformation.GetParamValueOrDefault<double>(
                SharedParamsConfig.Instance.VISPipeInsulationReserve) / 100;
        }
        if(category == BuiltInCategory.OST_DuctInsulations) {
            return 1 + _doc.ProjectInformation.GetParamValueOrDefault<double>(
                SharedParamsConfig.Instance.VISDuctInsulationReserve) / 100;
        }

        return 1;
    }
}
