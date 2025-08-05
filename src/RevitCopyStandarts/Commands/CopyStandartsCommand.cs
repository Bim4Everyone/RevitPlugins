using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitCopyStandarts.ViewModels;

namespace RevitCopyStandarts.Commands;

public abstract class CopyStandartsCommand : ICopyStandartsCommand {
    protected readonly Document _source;
    protected readonly Document _target;

    protected CopyStandartsCommand(Document source, Document target) {
        _source = source;
        _target = target;
    }

    public virtual string Name { get; set; }

    public void Execute() {
        using var transactionGroup = new TransactionGroup(_target);
        transactionGroup.BIMStart($"Копирование \"{Name}\"");

        var copyOptions = new CopyPasteOptions();
        // copyOptions.SetDuplicateTypeNamesHandler(GetDuplicateTypeNamesHandler());

        var elements = GetElements();
        foreach(var element in FilterElements(elements)) {
            using var transaction = new Transaction(_target);
            transaction.BIMStart($"Копирование \"{Name} - {element.Name}\"");

            var newElementId = CopyElement(element, copyOptions);
            if(IsAllowCommit(_target.GetElement(newElementId), element)) {
                transaction.Commit();
            } else {
                transaction.RollBack();
            }
        }

        transactionGroup.Assimilate();
    }

    protected virtual FilteredElementCollector GetFilteredElementCollector() {
        return new FilteredElementCollector(_source);
    }

    protected virtual IEnumerable<Element> GetElements() {
        return GetFilteredElementCollector().ToElements();
    }

    protected virtual ElementId CopyElement(Element element, CopyPasteOptions copyOptions) {
        return ElementTransformUtils.CopyElements(
                _source,
                [element.Id],
                _target,
                Transform.Identity,
                copyOptions)
            .First();
    }

    protected virtual bool IsAllowCommit(Element newElement, Element sourceElement) {
        return newElement.Name.Equals(sourceElement.Name);
    }

    protected virtual IDuplicateTypeNamesHandler GetDuplicateTypeNamesHandler() {
        return new CustomCopyHandler();
    }

    protected virtual IEnumerable<Element> FilterElements(IEnumerable<Element> elements) {
        return elements.Where(item => item.ViewSpecific == false);
    }
}
