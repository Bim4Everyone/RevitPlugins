using System.Collections.Generic;

using RevitEnumerateBySpline.Models.Settings;

namespace RevitEnumerateBySpline.Models.Interfaces;

/// <summary>Записывает номера и связи зависимых помещений.</summary>
internal interface IParamSetter {
    /// <summary>Записывает параметры помещений в заданном порядке.</summary>
    /// <param name="intersections">Помещения с первой точкой попадания сплайна.</param>
    /// <param name="settings">Настройки нумерации и зависимых помещений.</param>
    void SetParams(IReadOnlyList<SpatialPointIntersection> intersections, ConfigSettings settings);
}
