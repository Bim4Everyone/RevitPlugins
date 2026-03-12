using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface ISpatialElementDividerService {
    /// <summary>
    /// Метод разбивки зоны на квадраты.
    /// </summary>
    /// <remarks>
    /// В данном методе производится разбивка зоны на отдельные квадраты (сетка)
    /// с заданной длиной стороны и углом поворота.
    /// </remarks>
    /// <param name="spatialElement">Исходная зона.</param>
    /// <param name="side">Длина стороны квадрата.</param>
    /// <param name="angleDeg">Угол поворота в градусах.</param>
    /// <returns>
    /// Коллекция полигонов, представляющих фрагменты исходной зоны после разбиения.
    /// </returns>
    List<PolygonObject> DivideSpatialElement(SpatialElement spatialElement, double side, double angleDeg);
}
