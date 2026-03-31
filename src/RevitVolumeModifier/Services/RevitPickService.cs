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
    public async Task<Reference> PickPointOnElementAsync(string prompt) {
        _mainWindow.Hide();
        try {
            return await _handler.RaiseAsync(uiApp => {
                var uidoc = uiApp.ActiveUIDocument;
                try {
                    return uidoc.Selection.PickObject(ObjectType.PointOnElement, prompt);
                } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
                    return null;
                }
            });
        } finally {
            _mainWindow.Show();
        }
    }

    /// <summary>
    /// Выбор нескольких граней через PickObjects с фильтром Face
    /// </summary>
    public async Task<IList<Reference>> PickFacesMultipleOnElementAsync(string prompt) {
        _mainWindow.Hide();
        try {
            return await _handler.RaiseAsync(uiApp => {
                var uidoc = uiApp.ActiveUIDocument;
                try {
                    var faces = uidoc.Selection.PickObjects(
                        ObjectType.Face,
                        new FaceSelectionFilter(),
                        prompt);

                    return faces;
                } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
                    return null;
                }
            });
        } finally {
            _mainWindow.Show();
        }
    }

    /// <summary>
    /// Выбор элемента с фильтром DirectShape и "Модели в контексте"
    /// </summary>
    public async Task<IList<ElementId>> PickGenericModelsAsync(string prompt) {
        _mainWindow.Hide();
        try {
            return await _handler.RaiseAsync(uiApp => {
                var uidoc = uiApp.ActiveUIDocument;
                try {
                    var refs = uidoc.Selection.PickObjects(
                        ObjectType.Element,
                        new DirectShapeOrInPlaceFilter(uidoc.Document),
                        prompt);

                    return refs.Select(x => x.ElementId).ToList();
                } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
                    return null;
                }
            });
        } finally {
            _mainWindow.Show();
        }
    }
}
