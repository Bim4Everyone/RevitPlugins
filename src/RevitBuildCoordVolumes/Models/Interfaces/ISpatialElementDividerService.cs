using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface ISpatialElementDividerService {
    /// <summary>
    /// Метод разбивки зоны на отдельные фрагменты.
    /// </summary>
    /// <remarks>
    /// В данном методе производится разбивка зоны на отдельные участки
    /// с заданной длиной стороны и углом поворота.
    /// </remarks>
    /// <param name="spatialElement">Исходный пространственный элемент.</param>
    /// <param name="side">Длина стороны фрагмента.</param>
    /// <param name="angleRad">Угол поворота в радианах.</param>
    /// <returns>
    /// Коллекция полигонов, представляющих фрагменты исходной зоны после разбиения.
    /// </returns>
    List<PolygonObject> DivideSpatialElement(SpatialElement spatialElement, double side, double angleRad);
}
