using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

internal class DocumentMonitor {
    private readonly UIApplication _uiApplication;

    public event Action<Document> ActiveDocumentChanged;

    private Document _lastDocument;

    public DocumentMonitor(UIApplication uiApplication) {
        _uiApplication = uiApplication;
        _uiApplication.ViewActivated += OnViewActivated;
    }

    private void OnViewActivated(object sender, ViewActivatedEventArgs e) {
        var doc = e.Document;
        if(doc == null) {
            return;
        }

        if(!ReferenceEquals(_lastDocument, doc)) {
            _lastDocument = doc;
            ActiveDocumentChanged?.Invoke(doc);
        }
    }
}
