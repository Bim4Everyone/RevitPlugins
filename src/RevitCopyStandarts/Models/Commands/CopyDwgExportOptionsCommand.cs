using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitCopyStandarts.Models.Commands;

internal class CopyDwgExportOptionsCommand : ICopyStandartsCommand {
    private readonly Document _source;
    private readonly Document _target;
    private readonly ILocalizationService _localizationService;

    public CopyDwgExportOptionsCommand(Document source, Document target, ILocalizationService localizationService) {
        _source = source;
        _target = target;
        _localizationService = localizationService;
    }

    public void Execute() {
        var sourceOptions = DWGExportOptions.GetPredefinedOptions(_source, "");
        var targetOptions = DWGExportOptions.GetPredefinedOptions(_target, "");
    }
}
