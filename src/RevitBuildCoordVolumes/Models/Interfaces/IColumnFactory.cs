using System.Collections.Generic;
using System.Linq;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface IColumnFactory {
    /// <summary>
    /// Метод генерации групп колонн по их плитам.
    /// </summary>
    /// <remarks>
    /// В данном методе производится группировка колонн. Параметры группировки - низ и верх плит перекрытия.
    /// </remarks>
    /// <param name="polygons">Список полигонов, на которые была разбита зона.</param>    
    /// <param name="slabs">Список плит перекрытий, служащих для определения точек пересечения.</param>
    /// <returns>
    /// Коллекция групп ColumnObject, где ключ параметр группировки (GUID низа и верха плиты).
    /// </returns>
    IEnumerable<IGrouping<string, ColumnObject>> GenerateColumnGroups(List<PolygonObject> polygons, List<SlabElement> slabs);
}
