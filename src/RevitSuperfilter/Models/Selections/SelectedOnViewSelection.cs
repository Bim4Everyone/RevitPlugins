using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

using dosymep.Revit;

using RevitSuperfilter.Comparators;

namespace RevitSuperfilter.Models.Selections;

internal sealed class SelectedOnViewSelection : BaseSelection, ISelectionElements {
    private bool _onSelection;

    private readonly HashSet<ElementId> _selectedElementIds = [];
    private readonly HashSet<ElementId> _selectedRemovedElementIds = [];

    public SelectedOnViewSelection(UIApplication uiApplication)
        : base(uiApplication) {
        _uiApplication.Idling += UiApplicationOnIdling;
        _uiApplication.SelectionChanged += UiApplicationOnSelectionChanged;
    }

    public override Selection Selection => Selection.SelectedOnViewSelection;

    public override IEnumerable<Element> GetElements() {
        return UIDocument.GetSelectedElements();
    }

    protected override void OnDocumentChanged() {
        // do nothing
    }

    private void OnSelectionChange() {
        if(!_onSelection) {
            return;
        }

        _onSelection = false;

        var selectedElements = _selectedElementIds.ToList();
        var selectedRemovedElements = _selectedRemovedElementIds.ToList();

        _selectedRemovedElementIds.Clear();

        OnSelectionChange(
            new SelectionChangeEventArgs(
                selectedElements,
                selectedRemovedElements,
                []));
    }

    private void UiApplicationOnIdling(object sender, IdlingEventArgs e) {
        OnSelectionChange();
    }

    private void UiApplicationOnSelectionChanged(object sender, SelectionChangedEventArgs e) {
        var currentElements = e.GetSelectedElements();

        _selectedRemovedElementIds.UnionWith(_selectedElementIds);
        _selectedElementIds.Clear();
        _selectedElementIds.UnionWith(currentElements);

        _onSelection = true;
    }

    protected override void Dispose(bool disposing) {
        if(disposing) {
            _uiApplication.Idling -= UiApplicationOnIdling;
            _uiApplication.SelectionChanged -= UiApplicationOnSelectionChanged;
        }

        base.Dispose(disposing);
    }
}
