using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using RevitVolumeModifier.Handler;
using RevitVolumeModifier.Models;

namespace RevitVolumeModifier.Services;
/// <summary>
/// Сервис безопасного выбора объектов/точек в Revit из немодального окна
/// </summary>
internal class RevitPickService {
    private readonly ExternalRevitHandler _handler;
    private readonly Window _mainWindow;

    public RevitPickService(ExternalRevitHandler handler, Window mainWindow) {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
    }

    /// <summary>
    /// Выбор точки, привязанной к элементу
    /// </summary>
    public Task<Reference> PickPointOnElementAsync(string prompt) {
        var tcs = new TaskCompletionSource<Reference>();

        _handler.Raise(uiApp => {
            var uidoc = uiApp.ActiveUIDocument;
            try {
                _mainWindow.Dispatcher.Invoke(() => _mainWindow.Hide());

                var reference = uidoc.Selection.PickObject(ObjectType.PointOnElement, prompt);

                if(reference != null) {
                    tcs.SetResult(reference);
                } else {
                    tcs.SetResult(null);
                }
            } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
                tcs.SetResult(null);
            } finally {
                _mainWindow.Dispatcher.Invoke(() => _mainWindow.Show());
            }
        });

        return tcs.Task;
    }

    /// <summary>
    /// Выбор нескольких граней через PickObjects с фильтром Face
    /// </summary>    
    public Task<IList<Reference>> PickFacesMultipleOnElementAsync(string prompt) {
        var tcs = new TaskCompletionSource<IList<Reference>>();

        _handler.Raise(uiApp => {
            var uidoc = uiApp.ActiveUIDocument;
            try {
                _mainWindow.Dispatcher.Invoke(() => _mainWindow.Hide());

                var faces = uidoc.Selection.PickObjects(ObjectType.PointOnElement, new FaceSelectionFilter(), prompt);

                tcs.SetResult(faces);

            } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
                tcs.SetResult(null);
            } finally {
                _mainWindow.Dispatcher.Invoke(() => _mainWindow.Show());
            }
        });

        return tcs.Task;
    }

    /// <summary>
    /// Выбор элемента с фильтром DirectShape и "Модели в контексте"
    /// </summary>
    public Task<IList<ElementId>> PickGenericModelsAsync(string prompt) {
        var tcs = new TaskCompletionSource<IList<ElementId>>();
        _handler.Raise(uiApp => {
            var uidoc = uiApp.ActiveUIDocument;
            try {
                _mainWindow.Dispatcher.Invoke(() => _mainWindow.Hide());
                var refs = uidoc.Selection.PickObjects(ObjectType.Element, new DirectShapeOrInPlaceFilter(uidoc.Document), prompt);
                tcs.SetResult(refs.Select(x => x.ElementId).ToList());
            } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
                tcs.SetResult(null);
            } finally {
                _mainWindow.Dispatcher.Invoke(() => _mainWindow.Show());
            }
        });
        return tcs.Task;
    }
}
