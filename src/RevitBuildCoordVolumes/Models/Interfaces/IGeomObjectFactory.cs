using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface IGeomObjectFactory {
    /// <summary>
    /// Метод получения списка геометрических объектов по исходной зоне.
    /// </summary>
    /// <remarks>
    /// В данном методе производится получение геометрических объектов для построения DirectShape по исходной зоне.
    /// </remarks>
    /// <param name="spatialElement">Исходная зона.</param>   
    /// <param name="startExtrudePosition">Позиция для старта экструзии.</param>   
    /// <param name="finishExtrudePosition">Позиция для конца экструзии.</param>   
    /// <param name="basePointOffset">Смещение базовой точки проекта относительно внутреннего начала.</param>   
    /// <returns>
    /// Список геометрических элементов GeomObject.
    /// </returns>
    List<GeomObject> GetSimpleGeomObjects(
        SpatialElement spatialElement, double startExtrudePosition, double finishExtrudePosition, double basePointOffset);
    /// <summary>
    /// Метод получения списка геометрических объектов по списку колонн.
    /// </summary>
    /// <remarks>
    /// В данном методе производится получение объединенных геометрических объектов для построения DirectShape.
    /// </remarks>
    /// <param name="columns">Колонны, построенные ColumnFactory.</param>   
    /// <param name="spatialElementPosition">Реальная позиция кривой исходной зоны.</param> 
    /// <returns>
    /// Список геометрических элементов GeomObject.
    /// </returns>
    List<GeomObject> GetUnitedGeomObjects(List<ColumnObject> columns, double spatialElementPosition);
    /// <summary>
    /// Метод получения списка геометрических объектов по списку колонн.
    /// </summary>
    /// <remarks>
    /// В данном методе производится получение разъединенных геометрических объектов для построения DirectShape.
    /// </remarks>
    /// <param name="columns">Колонны, построенные ColumnFactory.</param>   
    /// <param name="spatialElementPosition">Реальная позиция кривой исходной зоны.</param> 
    /// <returns>
    /// Список геометрических элементов GeomObject.
    /// </returns>
    List<GeomObject> GetSeparatedGeomObjects(List<ColumnObject> columns, double spatialElementPosition);
}
