using System.Collections.Generic;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Services;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface ICoordVolumeBuilder {
    /// <summary>
    /// Основной метод для построение объемных элементов.
    /// </summary>
    /// <remarks>
    /// В данном методе производится построение объемных элементов GeomObject.
    /// </remarks>
    /// <param name="spatialElement">Исходная зона.</param>    
    /// <returns>
    /// Список GeomObject дальнейшего присвоение параметров.
    /// </returns>
    List<GeomObject> Build(SpatialObject spatialElement, ProgressService pro);
}
