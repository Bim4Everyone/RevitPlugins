using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitCopyStandarts.Models.Commands;

/// <summary>
///     Копирует легенды в документ
/// </summary>
internal class CopyViewLegendsCommand : ICopyStandartsCommand {
    private readonly Document _source;
    private readonly Document _target;

    public CopyViewLegendsCommand(Document source, Document target) {
        _source = source;
        _target = target;
    }

    public string Name => "Легенда";

    public void Execute() {
        var sourceElements = GetElements(_source).ToList();
        var targetElements = GetElements(_target).ToList();

        using var transactionGroup = new TransactionGroup(_target);
        transactionGroup.BIMStart($"Копирование \"{Name}\"");

        foreach(var sourceElement in sourceElements) {
            using var transaction = new Transaction(_target);
            transaction.BIMStart($"Копирование \"{Name} - {sourceElement.Name}\"");

            var newElement = _target.GetElement(CopyElement(sourceElement));
            var targetElement = targetElements.FirstOrDefault(item => item.Name.Equals(sourceElement.Name));

            if(targetElement != null) {
                ReplaceElement(newElement, targetElement);
            }

            transaction.Commit();
        }

        transactionGroup.Assimilate();
    }

    private void ReplaceElement(Element newElement, Element targetElement) {
        _target.Delete(GetElements(targetElement).Select(item => item.Id).ToArray());

        CopyElement((View) newElement, (View) targetElement);

        _target.Delete(newElement.Id);
    }

    private ElementId CopyElement(Element element) {
        return ElementTransformUtils.CopyElements(
                _source,
                [element.Id],
                _target,
                Transform.Identity,
                new CopyPasteOptions())
            .First();
    }

    private ElementId CopyElement(View newLegend, View targetLegend) {
        var legends = GetElements(newLegend).Select(item => item.Id).ToArray();
        if(legends.Length == 0) {
            return ElementId.InvalidElementId;
        }

        return ElementTransformUtils.CopyElements(
                newLegend,
                legends,
                targetLegend,
                Transform.Identity,
                new CopyPasteOptions())
            .First();
    }

    private IEnumerable<Element> GetElements(Element view) {
        return new FilteredElementCollector(view.Document, view.Id)
            .Where(item => item.Category != null);
    }

    private IEnumerable<Element> GetElements(Document document) {
        return new FilteredElementCollector(document)
            .OfClass(typeof(View))
            .ToElements()
            .Cast<View>()
            .Where(item => item.ViewType == ViewType.Legend);
    }
}
