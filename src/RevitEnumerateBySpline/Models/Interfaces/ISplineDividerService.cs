using System.Collections.Generic;

namespace RevitEnumerateBySpline.Models.Interfaces;

internal interface ISplineDividerService {
    /// <summary>Разбивает линии в порядке GUI на сегменты длиной до 10 мм.</summary>
    IReadOnlyList<SplineSegment> Divide(IEnumerable<CurveModel?> curveModels);
}
