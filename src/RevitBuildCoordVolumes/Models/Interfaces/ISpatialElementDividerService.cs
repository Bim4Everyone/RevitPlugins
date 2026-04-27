using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Settings;

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
    /// <param name="settings">Настройки проекта.</param>
    /// <returns>
    /// Коллекция полигонов, представляющих фрагменты исходной зоны после разбиения.
    /// </returns>
    List<PolygonObject> DivideSpatialElement(SpatialElement spatialElement, BuildCoordVolumeSettings settings, ProgressService pro);
}
