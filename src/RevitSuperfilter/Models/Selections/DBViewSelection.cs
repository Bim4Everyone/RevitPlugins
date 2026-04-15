using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

namespace RevitSuperfilter.Models.Selections;

internal sealed class DBViewSelection : BaseSelection, ISelectionElements {
    private bool _viewChanged;

    public DBViewSelection(UIApplication uiApplication)
        : base(uiApplication) {
        _uiApplication.Idling += UiApplicationOnIdling;
        _uiApplication.ViewActivated += UiApplicationOnViewActivated;
        _uiApplication.Application.DocumentChanged += ApplicationOnDocumentChanged;
    }

    private void UiApplicationOnIdling(object sender, IdlingEventArgs e) {
        ViewChanged();
    }

    public override Selection Selection => Selection.DBViewSelection;

    public override IEnumerable<Element> GetElements() {
        var document = Document;
        return new FilteredElementCollector(document, document.ActiveView.Id)
            .WhereElementIsNotElementType()
            .ToElements();
    }

    protected override void OnDocumentChanged() {
        if(_addedElements.Count == 0
           && _removedElements.Count == 0
           && _modifiedElements.Count == 0) {
            return;
        }

        var addedElements = GetElements(_addedElements);
        var removedElements = _removedElements.ToList();
        var modifiedElements = GetElements(_modifiedElements);

        _addedElements.Clear();
        _removedElements.Clear();
        _modifiedElements.Clear();

        OnSelectionChange(
            new SelectionChangeEventArgs(
                addedElements,
                removedElements,
                modifiedElements));
    }

    private IEnumerable<ElementId> GetElements(IEnumerable<ElementId> elementIds) {
        var document = Document;
        var elements = elementIds.ToList();
        if(elements.Count == 0) {
            return [];
        }
        
        return new FilteredElementCollector(document, document.ActiveView.Id)
            .WherePasses(new ElementIdSetFilter(elements))
            .ToElementIds();
    }

    private void ViewChanged() {
        if(!_viewChanged) {
            return;
        }

        _viewChanged = false;
        OnSelectionChange(new SelectionChangeEventArgs());
    }

    private void UiApplicationOnViewActivated(object sender, ViewActivatedEventArgs e) {
        OnSelectionChange(new SelectionChangeEventArgs());
    }

    private void ApplicationOnDocumentChanged(object sender, DocumentChangedEventArgs e) {
        if(e.Operation is UndoOperation.TransactionRolledBack or UndoOperation.TransactionGroupRolledBack) {
            return;
        }

        var document = Document;
        _viewChanged = e.GetModifiedElementIds().Contains(document.ActiveView.Id);
    }

    protected override void Dispose(bool disposing) {
        if(disposing) {
            _uiApplication.Idling -= UiApplicationOnIdling;
            _uiApplication.ViewActivated -= UiApplicationOnViewActivated;
            _uiApplication.Application.DocumentChanged -= ApplicationOnDocumentChanged;
        }

        base.Dispose(disposing);
    }
}
