using System.Collections.Generic;

using Autodesk.Revit.UI;

namespace RevitEnumerateBySpline.Models.Interfaces;

/// <summary>Запрашивает выбор кривых в Revit.</summary>
internal interface ISelectionService {
    /// <summary>Запрашивает выбор линий и сплайнов в модели.</summary>
    /// <param name="prompt">Подсказка во время выбора.</param>
    /// <param name="uiDoc">Документ Revit с активным выбором.</param>
    IList<CurveModel> PickCurve(string prompt, UIDocument uiDoc);
}
