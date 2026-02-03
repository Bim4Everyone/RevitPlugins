using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface IContourService {
    /// <summary>
    /// Метод получения объединенного контура по списку ColumnObject.
    /// </summary>
    /// <remarks>
    /// В данном методе производится объединение контура из ColumnObject, где они соединяются в единый контур
    /// </remarks>
    /// <param name="columns">Список колонн, которые были сформированы ColumnFactory.</param>    
    /// <param name="spatialElementPosition">Реальная позиция кривой исходной зоны.</param>
    /// <param name="startExtrudePosition">Позиция для старта экструзии.</param>
    /// <returns>
    /// Список CurveLoop для построения Solid.
    /// </returns>
    List<CurveLoop> GetColumnsCurveLoops(List<ColumnObject> columns, double spatialElementPosition, double startExtrudePosition);
    /// <summary>
    /// Метод получения контура по ColumnObject.
    /// </summary>
    /// <remarks>
    /// В данном методе производится получение контура из ColumnObject.
    /// </remarks>
    /// <param name="column">Колонна, которые были сформирована ColumnFactory.</param>    
    /// <param name="spatialElementPosition">Реальная позиция кривой исходной зоны.</param>
    /// <param name="startExtrudePosition">Позиция для старта экструзии.</param>
    /// <returns>
    /// Список CurveLoop для построения Solid.
    /// </returns>
    List<CurveLoop> GetColumnCurveLoops(ColumnObject column, double spatialElementPosition, double startExtrudePosition);
    /// <summary>
    /// Метод получения контура из исходной зоны.
    /// </summary>
    /// <remarks>
    /// В данном методе производится получение контура из исходной зоны. Берется только внешний контур.
    /// </remarks>
    /// <param name="spatialElement">Исходная зона.</param>    
    /// <param name="startExtrudePosition">Позиция для старта экструзии.</param>
    /// <param name="basePointOffset">Смещение базовой точки проекта относительно внутреннего начала.</param>
    /// <returns>
    /// Список CurveLoop для построения Solid.
    /// </returns>
    List<CurveLoop> GetSimpleCurveLoops(SpatialElement spatialElement, double startExtrudePosition, double basePointOffset);
    /// <summary>
    /// Метод получения внешнего контура из исходной зоны.
    /// </summary>
    /// <remarks>
    /// В данном методе производится получение внешнего контура из исходной зоны.
    /// </remarks>
    /// <param name="spatialElement">Исходная зона.</param>
    /// <returns>
    /// Список Curve для построения CurveLoop.
    /// </returns>
    List<Curve> GetOuterContour(SpatialElement spatialElement);
    /// <summary>
    /// Метод получения контура из набора кривых.
    /// </summary>
    /// <remarks>
    /// В данном методе производится получение замкнутого контура из исходных кривых.
    /// </remarks>
    /// <param name="allCurves">Исходный список кривых.</param>
    /// <param name="transform">Трансформация - разница реальной точки кривой и точки для экструзии.</param>
    /// <returns>
    /// Список CurveLoop для построения Solid.
    /// </returns>
    List<CurveLoop> GetCurveLoopsContour(List<Curve> allCurves, Transform transform);
}
