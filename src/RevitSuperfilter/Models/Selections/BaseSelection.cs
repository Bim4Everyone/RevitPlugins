using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

namespace RevitSuperfilter.Models.Selections;

internal abstract class BaseSelection : ISelectionElements, IDisposable {
    public event EventHandler<SelectionChangeEventArgs> OnSelectionChanged;

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

    private void UiApplicationOnIdling(object sender, IdlingEventArgs e) {
        var newDocument = Document;

        var oldModelPath = _document.GetWorksharingCentralModelPath();
        var newModelPath = newDocument.GetWorksharingCentralModelPath();

        if(oldModelPath != newModelPath) {
            OnSelectionChange(new SelectionChangeEventArgs());
        }
    }

    private void UiApplicationOnDocumentChanged(object sender, DocumentChangedEventArgs e) {
        if(e.Operation is UndoOperation.TransactionRolledBack or UndoOperation.TransactionGroupRolledBack) {
            return;
        }

        OnSelectionChange(
            new SelectionChangeEventArgs(
                e.GetAddedElementIds(),
                e.GetDeletedElementIds(),
                e.GetModifiedElementIds()));
    }

    protected virtual void Dispose(bool disposing) {
        if(disposing) {
            _uiApplication.Idling -= UiApplicationOnIdling;
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
