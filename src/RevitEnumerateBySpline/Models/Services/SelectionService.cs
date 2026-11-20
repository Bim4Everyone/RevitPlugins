using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using Ninject;
using Ninject.Syntax;

using RevitEnumerateBySpline.Models.Interfaces;
using RevitEnumerateBySpline.Views;

namespace RevitEnumerateBySpline.Models.Services;

internal class SelectionService(IResolutionRoot resolutionRoot) : ISelectionService {
    
    /// <summary>
    /// Последовательно выбирает ModelCurve и DetailCurve до подтверждения или отмены набора.
    /// </summary>
    /// <param name="prompt">Подсказка Revit во время выбора.</param>
    /// <param name="uiDoc">Документ Revit с активным выбором.</param>
    /// <param name="onPicked">Обработчик обновления предпросмотра после добавления линии.</param>
    /// <returns>Подтверждённые линии в порядке выбора или пустой список при отмене.</returns>
    public IList<CurveModel> PickCurve(string prompt, UIDocument uiDoc,
        Action<IReadOnlyList<CurveModel>>? onPicked = null) {
        var curves = new List<CurveModel>();
        var selectedIds = new HashSet<ElementId>();
        var originalSelection = uiDoc.Selection.GetElementIds().ToList();
        var panel = resolutionRoot.Get<CurveSelectionWindow>();
        panel.SetRevitOwner(uiDoc.Application.MainWindowHandle);
        try {
            panel.Show();
            var filter = new CurveSelectionFilter();
            while(!panel.Accepted.HasValue) {
                Reference reference;
                panel.BeginPick();
                try {
                    reference = uiDoc.Selection.PickObject(ObjectType.Element, filter, prompt);
                } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
                    // Escape вручную отменяет текущий набор. Кнопка «Готово»
                    // также прерывает PickObject, но подтверждает накопленные линии.
                    break;
                } finally {
                    panel.EndPick();
                }
                if(panel.Accepted.HasValue) {
                    break;
                }
                if(uiDoc.Document.GetElement(reference.ElementId) is not CurveElement curve
                   || !selectedIds.Add(reference.ElementId)) {
                    continue;
                }
                curves.Add(new CurveModel { ElementId = reference.ElementId, CurveElement = curve });
                onPicked?.Invoke(curves);
                panel.EnableFinish();
                uiDoc.Selection.SetElementIds(selectedIds.ToList());
                uiDoc.RefreshActiveView();
            }
            return panel.Accepted == true ? curves : [];
        } finally {
            panel.ClosePanel();
            if(uiDoc.Document.IsValidObject) {
                uiDoc.Selection.SetElementIds(originalSelection);
                uiDoc.RefreshActiveView();
            }
        }
    }
}
