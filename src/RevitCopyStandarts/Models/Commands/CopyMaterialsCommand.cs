using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitCopyStandarts.Models.Commands;

/// <summary>
///     Копирует материал Manage -> Materials
/// </summary>
internal class CopyMaterialsCommand : CopyStandartsCommand {
    public CopyMaterialsCommand(Document source, Document destination, ILocalizationService localizationService)
        : base(source, destination, localizationService) {
    }

    public override string Name => _localizationService.GetLocalizedString("CopyMaterialsCommandName");

    protected override FilteredElementCollector GetFilteredElementCollector() {
        return base.GetFilteredElementCollector()
            .OfClass(typeof(Material));
    }
}
