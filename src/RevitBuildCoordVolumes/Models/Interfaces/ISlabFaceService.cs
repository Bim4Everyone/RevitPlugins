using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Interfaces;

public interface ISlabFaceService {
    /// <summary>
    /// Получение реальных верхних граней плиты.
    /// </summary>
    /// <remarks>
    /// В данном методе извлекаются солиды плиты, при необходимости преобразуются
    /// в заданную систему координат, после чего из них выбираются грани,
    /// нормаль которых направлена вверх.
    /// </remarks>
    /// <param name="floor">Плита, верхние грани которой необходимо получить.</param>
    /// <param name="transformFromDoc">Трансформация из системы координат исходного документа или <c>null</c>,
    /// если преобразование не требуется.</param>
    /// <returns>Список верхних граней плиты.</returns>
    List<Face> GetRealTopFaces(Floor floor, Transform transformFromDoc);
    /// <summary>
    /// Получение верхних граней плоской плиты.
    /// </summary>
    /// <remarks>
    /// В данном методе из солидов выбираются грани, нормаль которых направлена вверх
    /// в пределах заданного диапазона по оси Z.
    /// </remarks>
    /// <param name="solids">Список контуров и соответствующих им солидов.</param>
    /// <returns>Список граней, нормаль которых направлена вверх.</returns>
    List<Face> GetFlatTopFaces(IEnumerable<(CurveLoop Loop, Solid Solid)> solids);
    /// <summary>
    /// Получение максимальной отметки верхних граней.
    /// </summary>
    /// <remarks>
    /// В данном методе выполняется перебор граничных контуров граней
    /// и определяется максимальная координата Z среди конечных точек ребер.
    /// </remarks>
    /// <param name="topFaces">Список верхних граней.</param>
    /// <returns>Максимальная координата Z среди конечных точек ребер граней.</returns>
    double GetMaxPointZ(List<Face> topFaces);
    /// <summary>
    /// Получение минимальной отметки верхних граней.
    /// </summary>
    /// <remarks>
    /// В данном методе выполняется перебор граничных контуров граней
    /// и определяется минимальная координата Z среди конечных точек ребер.
    /// </remarks>
    /// <param name="topFaces">Список верхних граней.</param>
    /// <returns>Минимальная координата Z среди конечных точек ребер граней.</returns>
    double GetMinPointZ(List<Face> topFaces);
}
