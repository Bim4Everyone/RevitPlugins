using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitCopyStandarts.Models.Commands;

internal class CopyWallTypesCommand : CopyStandartsCommand {
    public CopyWallTypesCommand(Document source, Document target)
        : base(source, target) {
    }

    public override string Name { get; set; } = "Типы стен";

    protected override IEnumerable<Element> FilterElements(IEnumerable<Element> elements) {
        return elements.Cast<WallType>().Where(item => item.ViewSpecific == false && item.Kind == WallKind.Basic);
    }

    protected override FilteredElementCollector GetFilteredElementCollector() {
        return base.GetFilteredElementCollector()
            .OfClass(typeof(WallType));
    }
}
