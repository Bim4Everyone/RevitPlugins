using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace RevitEnumerateBySpline.Models.Services;

internal class SelectionService {
    
    /// <summary>
    /// Выбор элемента с фильтром ModelCurve и DetailCurve
    /// </summary>
    public IList<CurveModel> PickCurve(string prompt, UIDocument uiDoc) {
        try {
            var refs = uiDoc.Selection.PickObjects(
                ObjectType.Element,
                new CurveSelectionFilter(uiDoc.Document),
                prompt);

            return refs
                .Select(reference => {
                    var curveElement = uiDoc.Document.GetElement(reference.ElementId) as CurveElement;

                    return new CurveModel {
                        ElementId = reference.ElementId,
                        CurveElement = curveElement
                    };
                })
                .Where(x => x.CurveElement is not null)
                .ToList();

        } catch {
            return [];
        }
    }
}
