using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitPylonLoadAreas.Models.Geometry;

/// <summary>
/// Грузовая площадь заданного вертикального несущего элемента
/// </summary>
internal sealed class LoadArea {
    /// <summary>
    /// Конструирует грузовую площадь
    /// </summary>
    /// <param name="element">Вертикальный элемент</param>
    /// <param name="circuits">Список контуров грузовой площади. Первый контур - наружный, остальные - отверстия</param>
    public LoadArea(Element element, IList<CurveLoop> circuits) {
        Element = element ?? throw new ArgumentNullException(nameof(element));
        Circuits = circuits ?? throw new ArgumentNullException(nameof(circuits));
    }

    public Element Element { get; }

    public IList<CurveLoop> Circuits { get; }
}
