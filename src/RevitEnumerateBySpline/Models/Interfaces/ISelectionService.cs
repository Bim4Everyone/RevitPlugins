using System.Collections.Generic;

using System;

using Autodesk.Revit.UI;

namespace RevitEnumerateBySpline.Models.Interfaces;

/// <summary>Запрашивает выбор кривых в Revit.</summary>
internal interface ISelectionService {
    /// <summary>Запрашивает выбор линий и сплайнов в модели.</summary>
    /// <param name="prompt">Подсказка во время выбора.</param>
    /// <param name="uiDoc">Документ Revit с активным выбором.</param>
    /// <param name="onPicked">Обновляет предпросмотр после добавления новой линии.</param>
    /// <returns>Подтверждённые линии в порядке выбора или пустой список при отмене.</returns>
    IList<CurveModel> PickCurve(string prompt, UIDocument uiDoc,
        Action<IReadOnlyList<CurveModel>>? onPicked = null);
}
