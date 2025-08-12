using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitCopyStandarts.Models.Commands;

internal class CopyWallTypesCommand : CopyStandartsCommand {
    public CopyWallTypesCommand(Document source, Document target, ILocalizationService localizationService)
        : base(source, target, localizationService) {
    }

    public override string Name => _localizationService.GetLocalizedString("CopyWallTypesCommandName");

    protected override IEnumerable<Element> FilterElements(IEnumerable<Element> elements) {
        return elements.Cast<WallType>().Where(item => item.ViewSpecific == false && item.Kind == WallKind.Basic);
    }

    protected override FilteredElementCollector GetFilteredElementCollector() {
        return base.GetFilteredElementCollector()
            .OfClass(typeof(WallType));
    }
}
