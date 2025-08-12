using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitCopyStandarts.Models.Commands;

internal class CopyFoundationSlabCommand : CopyStandartsCommand {
    public CopyFoundationSlabCommand(Document source, Document target, ILocalizationService localizationService)
        : base(source, target, localizationService) {
    }

    public override string Name => _localizationService.GetLocalizedString("CopyFoundationSlabCommandName");

    protected override IEnumerable<Element> FilterElements(IEnumerable<Element> elements) {
        return elements.Cast<FloorType>().Where(item => item.IsFoundationSlab);
    }

    protected override FilteredElementCollector GetFilteredElementCollector() {
        return base.GetFilteredElementCollector()
            .OfClass(typeof(FloorType));
    }
}
