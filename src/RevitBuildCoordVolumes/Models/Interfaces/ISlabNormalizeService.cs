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
    /// <summary>
    /// Метод определения, наклонное ли перекрытие.
    /// </summary>
    /// <remarks>
    /// В данном методе производится проверка является ли плита наклонной или плоской. 
    /// Проверяется изменение формы и уклон с помощью стрелки уклона.
    /// </remarks>           
    /// <param name="slabElement">Перекрытие, которое нужно определить.</param>        
    /// <returns>
    /// True - перекрытие наклонное, False - горизонтальное
    /// </returns>
    bool IsSloped(SlabElement slabElement);
}
