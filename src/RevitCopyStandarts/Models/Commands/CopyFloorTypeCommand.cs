using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitCopyStandarts.Models.Commands;

internal class CopyFloorTypeCommand : CopyStandartsCommand {
    public CopyFloorTypeCommand(Document source, Document target, ILocalizationService localizationService)
        : base(source, target, localizationService) {
    }

    public override string Name => _localizationService.GetLocalizedString("CopyFloorTypeCommandName");

    protected override IEnumerable<Element> FilterElements(IEnumerable<Element> elements) {
        return elements.Cast<FloorType>().Where(item => item.IsFoundationSlab == false);
    }

    protected override FilteredElementCollector GetFilteredElementCollector() {
        return base.GetFilteredElementCollector()
            .OfClass(typeof(FloorType));
    }
}
