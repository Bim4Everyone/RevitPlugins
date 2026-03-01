using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitVolumeModifier.Enums;
using RevitVolumeModifier.Handler;

namespace RevitVolumeModifier.Models;

internal class RevitRepository {
    private readonly ILocalizationService _localizationService;
    private readonly RevitEventHandler _revitEventHandler;

    public RevitRepository(
        UIApplication uiApplication,
        ILocalizationService localizationService) {

        UIApplication = uiApplication;
        _localizationService = localizationService;
        _revitEventHandler = new RevitEventHandler();
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;

    public IEnumerable<string> GetElementsValues(ICollection<ElementId> selection, ParamModel param) {
        if(selection == null || !selection.Any() || param?.RevitParam == null) {
            return [_localizationService.GetLocalizedString("RevitRepository.NoParamValue")];
        }

        string paramName = param.RevitParam.Name;

        return selection
            .Select(elementId => {
                var element = Document.GetElement(elementId);
                return element == null || !element.IsExistsParam(paramName)
                    ? null
                    : param.ParamType switch {
                        ParamType.VolumeParam =>
                            element.GetParamValueOrDefault<double>(paramName).ToString(),

                        ParamType.FloorDEParam =>
                            element.GetParamValueOrDefault<double>(paramName).ToString(),

                        _ =>
                            element.GetParamValueOrDefault<string>(paramName)
                    };
            })
            .Where(str => !string.IsNullOrWhiteSpace(str))
            .Distinct();
    }

    public string GetVolume(ICollection<ElementId> selection, ParamModel param) {
        if(selection == null || !selection.Any() || param?.RevitParam == null) {
            return _localizationService.GetLocalizedString("RevitRepository.NoParamValue");
        }

        string paramName = param.RevitParam.Name;

        var volumes = selection
            .Select(elementId => {
                var element = Document.GetElement(elementId);
                return element == null || !element.IsExistsParam(paramName)
                    ? 0
                    : element.GetParamValueOrDefault<double>(paramName);
            });

        double sumVolumes = volumes.Sum();
        return sumVolumes > 0
            ? sumVolumes.ToString()
            : _localizationService.GetLocalizedString("RevitRepository.NoParamValue");
    }
}
