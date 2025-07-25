using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitSleeves.Models;

namespace RevitSleeves.Services.Core;
internal class AllMepElementsProvider : IMepElementsProvider {
    private readonly RevitRepository _revitRepository;

    public AllMepElementsProvider(RevitRepository revitRepository) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
    }

    public ICollection<ElementId> GetMepElementIds(BuiltInCategory category) {
        return new FilteredElementCollector(_revitRepository.Document)
            .WhereElementIsNotElementType()
            .OfCategory(category)
            .ToElementIds();
    }

    public ICollection<Element> GetMepElements(BuiltInCategory category) {
        return new FilteredElementCollector(_revitRepository.Document)
            .WhereElementIsNotElementType()
            .OfCategory(category)
            .ToElements();
    }
}
