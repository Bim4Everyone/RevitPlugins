using System.Collections.Generic;

using Autodesk.Revit.UI;

namespace RevitEnumerateBySpline.Models.Interfaces;

internal interface ISelectionService {
    /// <summary>Запрашивает выбор линий и сплайнов в модели.</summary>
    IList<CurveModel> PickCurve(string prompt, UIDocument uiDoc);
}
