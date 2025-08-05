using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitCopyStandarts.Models.Commands;

internal class CopyFoundationSlabCommand : CopyStandartsCommand {
    public CopyFoundationSlabCommand(Document source, Document target)
        : base(source, target) {
    }

    public override string Name => "Параметры ребра плиты";

    protected override IEnumerable<Element> FilterElements(IEnumerable<Element> elements) {
        return elements.Cast<FloorType>().Where(item => item.IsFoundationSlab);
    }

    protected override FilteredElementCollector GetFilteredElementCollector() {
        return base.GetFilteredElementCollector()
            .OfClass(typeof(FloorType));
    }
}
