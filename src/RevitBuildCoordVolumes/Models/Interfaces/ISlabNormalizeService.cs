using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface ISlabNormalizeService {
    /// <summary>
    /// Метод получения верхних граней перекрытия.
    /// </summary>
    /// <remarks>
    /// В данном методе производится получение верхних граней перекрытия, как плоских так и наклонных.
    /// </remarks>
    /// <param name="slabElement">Перекрытие.</param>        
    /// <returns>
    /// Список верхних граней Face.
    /// </returns>
    List<Face> GetTopFaces(SlabElement slabElement);
    /// <summary>
    /// Метод получения верхних граней перекрытия без отверстий и вырезов.
    /// </summary>
    /// <remarks>
    /// В данном методе производится получение верхних граней перекрытия без отверстий и вырезов. 
    /// Только для плоских перекрытий.
    /// </remarks>
    /// <param name="slabElement">Перекрытие.</param>        
    /// <param name="topFaces">Список верхних граней Face.</param>        
    /// <returns>
    /// Список верхних граней Face без отверстий и вырезов.
    /// </returns>
    List<Face> GetTopFacesClean(SlabElement slabElement, List<Face> topFaces);
}
