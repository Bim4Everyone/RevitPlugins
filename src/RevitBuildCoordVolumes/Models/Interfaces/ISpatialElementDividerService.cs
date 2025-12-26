using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface ISpatialElementDividerService {
    /// <summary>
    /// Метод разбивки зоны на отдельные фрагменты
    /// </summary>    
    /// <remarks>
    /// В данном методе производится разбивка зоны на отдельные участки с заданной длиной стороны и допуском Revit
    /// </remarks>
    /// <returns>List Polygon</returns>
    List<Polygon> DivideSpatialElement(SpatialElement spatialElement, double side);
}
