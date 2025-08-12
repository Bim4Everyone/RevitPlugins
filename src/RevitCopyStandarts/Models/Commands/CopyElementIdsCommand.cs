using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitCopyStandarts.Models.Commands;

internal class CopyElementIdsCommand : CopyStandartsCommand {
    public CopyElementIdsCommand(Document source, Document target, ILocalizationService localizationService)
        : base(source, target, localizationService) {
    }

    public override string Name => _localizationService.GetLocalizedString("CopyElementIdsCommandName");
    public IReadOnlyCollection<Element> CopyElements { get; set; }

    protected override IEnumerable<Element> GetElements() {
        return CopyElements;
    }
}
