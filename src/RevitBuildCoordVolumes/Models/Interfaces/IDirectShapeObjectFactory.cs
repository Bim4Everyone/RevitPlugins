using System.Collections.Generic;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface IDirectShapeObjectFactory {
    /// <summary>
    /// Основной метод для построения объемных элементов Revit.
    /// </summary>
    /// <remarks>
    /// В данном методе производится построение объемных элементов Revit - DirectShape.
    /// </remarks>
    /// <param name="geomObjects">Список объемных элементов GeomObject.</param>    
    /// <param name="revitRepository">Репозиторий Revit.</param>    
    /// <returns>
    /// Список DirectShapeObject дальнейшего присвоения параметров.
    /// </returns>
    List<DirectShapeObject> CreateDirectShapeObjects(List<GeomObject> geomObjects, RevitRepository revitRepository);
}
