using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitCopyStandarts.Models.Commands;

internal class CopyCurtainTypesCommand : ICopyStandartsCommand {
    private readonly Document _source;
    private readonly Document _target;
    private readonly ILocalizationService _localizationService;

    public CopyCurtainTypesCommand(Document source, Document target, ILocalizationService localizationService) {
        _source = source;
        _target = target;
        _localizationService = localizationService;
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
        transaction.BIMStart(_localizationService.GetLocalizedString("CopyCurtainTypesCommandTransaction"));

        ElementTransformUtils.CopyElements(
            _source,
            elements,
            _target,
            Transform.Identity,
            new CopyPasteOptions());

        transaction.Commit();
    }
}
