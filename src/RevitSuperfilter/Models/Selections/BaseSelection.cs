using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

namespace RevitSuperfilter.Models.Selections;

internal abstract class BaseSelection : ISelectionElements, IDisposable {
    public event EventHandler<SelectionChangeEventArgs> OnSelectionChanged;

    protected readonly HashSet<ElementId> _addedElements = [];
    protected readonly HashSet<ElementId> _removedElements = [];
    protected readonly HashSet<ElementId> _modifiedElements = [];

    protected readonly Document _document;
    protected readonly UIApplication _uiApplication;

    protected BaseSelection(UIApplication uiApplication) {
        _uiApplication = uiApplication;
        _document = Document;
        _uiApplication.Idling += UiApplicationOnIdling;
        _uiApplication.Application.DocumentChanged += UiApplicationOnDocumentChanged;
    }

    public abstract Selection Selection { get; }
    public abstract IEnumerable<Element> GetElements();

    protected UIDocument UIDocument => _uiApplication.ActiveUIDocument;
    protected Document Document => _uiApplication.ActiveUIDocument.Document;

    protected void OnSelectionChange(SelectionChangeEventArgs eventArgs) {
        OnSelectionChanged?.Invoke(this, eventArgs);
    }

    protected virtual void OnDocumentChanged() {
        if(_addedElements.Count == 0
           && _removedElements.Count == 0
           && _modifiedElements.Count == 0) {
            return;
        }

        var addedElements = _addedElements.ToList();
        var removedElements = _removedElements.ToList();
        var modifiedElements = _modifiedElements.ToList();

        _addedElements.Clear();
        _removedElements.Clear();
        _modifiedElements.Clear();

        OnSelectionChange(
            new SelectionChangeEventArgs(
                addedElements,
                removedElements,
                modifiedElements));
    }

    protected virtual void OnNewDocumentOpened() {
        var newDocument = Document;

        var oldModelPath = _document.GetWorksharingCentralModelPath();
        var newModelPath = newDocument.GetWorksharingCentralModelPath();

        if(oldModelPath != newModelPath) {
            OnSelectionChange(new SelectionChangeEventArgs());
        }
    }

    private void UiApplicationOnIdling(object sender, IdlingEventArgs e) {
        OnDocumentChanged();
        OnNewDocumentOpened();
    }

    private void UiApplicationOnDocumentChanged(object sender, DocumentChangedEventArgs e) {
        if(e.Operation is UndoOperation.TransactionRolledBack or UndoOperation.TransactionGroupRolledBack) {
            return;
        }

        _addedElements.UnionWith(e.GetAddedElementIds());
        _removedElements.UnionWith(e.GetDeletedElementIds());
        _modifiedElements.UnionWith(e.GetModifiedElementIds());
    }

    protected virtual void Dispose(bool disposing) {
        if(disposing) {
            _uiApplication.Idling -= UiApplicationOnIdling;
            _uiApplication.Application.DocumentChanged -= UiApplicationOnDocumentChanged;
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
