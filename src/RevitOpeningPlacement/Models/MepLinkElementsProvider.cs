using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models;
internal class MepLinkElementsProvider : IMepLinkElementsProvider {
    private readonly ICollection<ElementId> _mepElements;
    private readonly ICollection<ElementId> _openingTasks;


    /// <summary>
    /// Конструктор провайдера элементов ВИС из связанного файла
    /// </summary>
    /// <param name="linkDocument">Связанный файл с элементами ВИС</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public MepLinkElementsProvider(RevitLinkInstance linkDocument) {
        if(linkDocument is null) {
            throw new ArgumentNullException(nameof(linkDocument));
        }

        Document = linkDocument.GetLinkDocument();
        DocumentTransform = linkDocument.GetTransform();
        _mepElements = GetMepElementIds(Document);
        _openingTasks = GetOpeningsTaskIds(Document);
    }

    public Document Document { get; }

    public Transform DocumentTransform { get; }

    public ICollection<ElementId> GetMepElementIds() {
        return _mepElements;
    }

    public ICollection<ElementId> GetOpeningsTaskIds() {
        return _openingTasks;
    }


    private ICollection<ElementId> GetMepElementIds(Document document) {
        return new FilteredElementCollector(document)
            .WhereElementIsNotElementType()
            .WherePasses(FiltersInitializer.GetFilterByAllUsedMepCategories())
            .ToElementIds();
    }

    private ICollection<ElementId> GetOpeningsTaskIds(Document document) {
        var types = Enum.GetValues(typeof(OpeningType));
        return new FilteredElementCollector(document)
            .OfCategory(BuiltInCategory.OST_GenericModel)
            .OfClass(typeof(FamilyInstance))
            .Cast<FamilyInstance>()
            .Where(item => RevitRepository.OpeningTaskTypeName.Values.Any(value => value.Equals(item.Symbol.Name))
            && RevitRepository.OpeningTaskFamilyName.Values.Any(value => value.Equals(item.Symbol.FamilyName)))
            .Select(famInst => famInst.Id)
            .ToHashSet();
    }
}
