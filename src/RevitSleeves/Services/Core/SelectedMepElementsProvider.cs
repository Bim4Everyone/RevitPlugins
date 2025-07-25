using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using dosymep.SimpleServices;

using RevitSleeves.Models;

namespace RevitSleeves.Services.Core;
internal class SelectedMepElementsProvider : IMepElementsProvider {
    private readonly RevitRepository _revitRepository;
    private readonly MepSelectionFilter _filter;
    private readonly ILocalizationService _localizationService;
    private Element[] _selectedElements;

    public SelectedMepElementsProvider(
        RevitRepository revitRepository,
        MepSelectionFilter filter,
        ILocalizationService localizationService) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _filter = filter ?? throw new ArgumentNullException(nameof(filter));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }


    public ICollection<ElementId> GetMepElementIds(BuiltInCategory category) {
        SetSelectedItems();
        return [.. _selectedElements.Select(e => e.Id)];
    }

    public ICollection<Element> GetMepElements(BuiltInCategory category) {
        SetSelectedItems();
        return _selectedElements;
    }

    private void SetSelectedItems() {
        if(_selectedElements is not null) {
            return;
        }
        var references = _revitRepository.ActiveUIDocument.Selection.PickObjects(
            ObjectType.Element,
            _filter,
            _localizationService.GetLocalizedString("RevitUI.PickMepElements"));
        _selectedElements = [.. references.Where(r => r is not null).Select(_revitRepository.Document.GetElement)];
    }
}
