using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitCopyStandarts.Models.Commands;

internal class CopyFamiliesCommand : CopyStandartsCommand {
    public CopyFamiliesCommand(Document source, Document destination, ILocalizationService localizationService)
        : base(source, destination, localizationService) {
    }

    public override string Name => _localizationService.GetLocalizedString("CopyFamiliesCommandName");

    protected override FilteredElementCollector GetFilteredElementCollector() {
        return base.GetFilteredElementCollector()
            .OfClass(typeof(Family));
    }

    protected override bool IsAllowCommit(Element newElement, Element sourceElement) {
        return newElement.Name.Equals(sourceElement.Name);
    }
}
