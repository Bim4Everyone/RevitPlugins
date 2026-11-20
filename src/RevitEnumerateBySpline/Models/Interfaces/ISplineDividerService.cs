using System.Collections.Generic;

namespace RevitEnumerateBySpline.Models.Interfaces;

/// <summary>Разбивает выбранные кривые на точки.</summary>
internal interface ISplineDividerService {
    /// <summary>Разбивает линии в порядке GUI на точки с шагом.</summary>
    /// <param name="curveModels">Кривые в порядке нумерации.</param>
    IReadOnlyList<SplinePoint> Divide(IEnumerable<CurveModel?> curveModels);
}
