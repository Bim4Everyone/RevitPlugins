using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using dosymep.SimpleServices;

using RevitVolumeModifier.Handler;
using RevitVolumeModifier.Models;

namespace RevitVolumeModifier.Services;

/// <summary>
/// Сервис безопасного выбора объектов/точек в Revit из немодального окна
/// </summary>
internal class RevitPickService {
    private readonly ILocalizationService _localizationService;
    private readonly ExternalRevitHandler _handler;
    private readonly Window _mainWindow;

    public RevitPickService(ILocalizationService localizationService, ExternalRevitHandler handler, Window mainWindow) {
        _localizationService = localizationService;
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
                    throw new OperationCanceledException(
                        _localizationService.GetLocalizedString("RevitPickService.CancelledSelectionPoint"));
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
                        ObjectType.PointOnElement,
                        new FaceSelectionFilter(),
                        prompt);

                    return faces;
                } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
                    throw new OperationCanceledException(
                        _localizationService.GetLocalizedString("RevitPickService.CancelledSelectionFaces"));
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
                    throw new OperationCanceledException(
                        _localizationService.GetLocalizedString("RevitPickService.CancelledSelectionElements"));
                }
            });
        } finally {
            _mainWindow.Show();
        }
    }
}
