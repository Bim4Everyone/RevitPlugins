using System;
using System.Collections.Generic;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;

using RevitSuperfilter.Handlers;

namespace RevitSuperfilter.Models;

/// <summary>
/// Класс доступа к документу и приложению Revit.
/// </summary>
/// <remarks>
/// В случае если данный класс разрастается, рекомендуется его разделить на несколько.
/// </remarks>
internal class RevitRepository {
    private readonly RevitEventHandler _revitEventHandler = new();
    /// <summary>
    /// Создает экземпляр репозитория.
    /// </summary>
    /// <param name="uiApplication">Класс доступа к интерфейсу Revit.</param>
    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;
    }

    /// <summary>
    /// Класс доступа к интерфейсу Revit.
    /// </summary>
    public UIApplication UIApplication { get; }
    
    /// <summary>
    /// Класс доступа к интерфейсу документа Revit.
    /// </summary>
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    
    /// <summary>
    /// Класс доступа к приложению Revit.
    /// </summary>
    public Application Application => UIApplication.Application;
    
    /// <summary>
    /// Класс доступа к документу Revit.
    /// </summary>
    public Document Document => ActiveUIDocument.Document;

    public void SelectElements(ICollection<ElementId> elementIds) {
        if(elementIds.Count == 0) {
            return;
        }

        ActiveUIDocument.Selection.SetElementIds(elementIds);
    }

    public void ShowElements(ICollection<ElementId> elementIds) {
        if(elementIds.Count == 0) {
            return;
        }

        ActiveUIDocument.ShowElements(elementIds);
    }

    public void IsolateElements(ICollection<ElementId> elementIds) {
        if(elementIds.Count == 0) {
            return;
        }

        _revitEventHandler.TransactAction = () => {
            try {
                using var t = Document.StartTransaction("Изолировать элементы");
                Document.ActiveView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
                Document.ActiveView.IsolateElementsTemporary(elementIds);
                t.Commit();
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
            }
        };
        _revitEventHandler.Raise();
    }
}
