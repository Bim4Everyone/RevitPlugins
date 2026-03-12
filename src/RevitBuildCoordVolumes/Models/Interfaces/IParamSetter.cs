using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface IParamSetter {
    /// <summary>
    /// Метод назначения параметров.
    /// </summary>
    /// <remarks>
    /// В данном методе производится назначение параметров, полученных из исходной зоны.
    /// </remarks>
    /// <param name="spatialElement">Исходная зона.</param>    
    /// <param name="directShapeElements">Список объемных элементов Revit - DirectShape.</param>
    /// <param name="buildCoordVolumesSettings">Основной класс с настройками плагина.</param>
    /// <returns>
    /// Void.
    /// </returns>
    void SetParams(
        SpatialElement spatialElement,
        List<DirectShapeObject>
        directShapeElements,
        BuildCoordVolumeSettings buildCoordVolumesSettings);
}
