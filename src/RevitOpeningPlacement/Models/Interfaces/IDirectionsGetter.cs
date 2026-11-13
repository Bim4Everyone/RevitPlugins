using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Interfaces;
internal interface IDirectionsGetter {
    /// <summary>
    /// Возвращает векторы смещения параллельные заданной плоскости
    /// </summary>
    /// <param name="plane">Плоскость</param>
    /// <returns>Векторы, параллельные заданной плоскости</returns>
    IEnumerable<XYZ> GetDirectionsOnPlane(Plane plane);
}
