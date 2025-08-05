using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitCopyStandarts.Models.Commands;

/// <summary>
///     Данный класс копирует цветовые схемы Architecture -> Room & Area -> Color Schemes
/// </summary>
internal class CopyColorFillSchemesCommand : ICopyStandartsCommand {
    private readonly Document _source;
    private readonly Document _target;
    private readonly ILocalizationService _localizationService;

    public CopyColorFillSchemesCommand(Document source, Document target, ILocalizationService localizationService) {
        _source = source;
        _target = target;
        _localizationService = localizationService;
    }

    public void Execute() {
        var sourceElements = new FilteredElementCollector(_source)
            .OfCategory(BuiltInCategory.OST_ColorFillSchema)
            .ToElements();

        var targetElements = new FilteredElementCollector(_target)
            .OfCategory(BuiltInCategory.OST_ColorFillSchema)
            .ToElements();

        using var transaction = new Transaction(_target);
        transaction.BIMStart(_localizationService.GetLocalizedString("CopyColorFillSchemesCommandTransaction"));

        // если не удалять цветовые схемы,
        // то они копируются неверно, появляется дубликат без настроек
        _target.Delete(
            targetElements.Intersect(sourceElements, new FillSchemaEqualityComparer())
                .Select(item => item.Id)
                .ToArray());

        ElementTransformUtils.CopyElements(
            _source,
            sourceElements.Select(item => item.Id).ToArray(),
            _target,
            Transform.Identity,
            new CopyPasteOptions());

        transaction.Commit();
    }
}

internal class FillSchemaEqualityComparer : IEqualityComparer<Element> {
    public bool Equals(Element x, Element y) {
        return x?.Name.Equals(y?.Name) == true;
    }

    public int GetHashCode(Element obj) {
        return obj?.Name.GetHashCode() ?? 0;
    }
}
