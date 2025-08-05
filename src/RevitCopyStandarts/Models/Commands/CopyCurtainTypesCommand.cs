using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitCopyStandarts.Models.Commands;

internal class CopyCurtainTypesCommand : ICopyStandartsCommand {
    private readonly Document _source;
    private readonly Document _target;

    public CopyCurtainTypesCommand(Document source, Document target) {
        _source = source;
        _target = target;
    }

    public void Execute() {
        IList<ElementId> elements = new FilteredElementCollector(_source)
            .OfClass(typeof(WallType))
            .ToElements()
            .Cast<WallType>()
            .Where(item => item.ViewSpecific == false && item.Kind == WallKind.Curtain)
            .Select(item => item.Id)
            .ToList();

        using var transaction = new Transaction(_target);
        transaction.BIMStart("Копирование \"Типы витражей\"");

        ElementTransformUtils.CopyElements(
            _source,
            elements,
            _target,
            Transform.Identity,
            new CopyPasteOptions());

        transaction.Commit();
    }
}
