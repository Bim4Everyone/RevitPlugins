using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface ISlabsService {
    /// <summary>
    /// Метод получения плит
    /// </summary>    
    /// <remarks>
    /// В данном методе производится получения плит перекрытий по их именам типов и документам
    /// </remarks>
    /// <returns>IEnumerable SlabElement</returns>
    IEnumerable<SlabElement> GetSlabsByTypesAndDocs(IEnumerable<string> typeSlabs, IEnumerable<Document> documents);
    /// <summary>
    /// Метод получения плит
    /// </summary>    
    /// <remarks>
    /// В данном методе производится получения плит перекрытий по их именам типов, документам и диапазону уровней
    /// </remarks>
    /// <returns>IEnumerable SlabElement</returns>
    IEnumerable<SlabElement> GetSlabsByTypesDocsAndLevels(
        IEnumerable<string> typeSlabs,
        IEnumerable<Document> documents,
        Level upLevel,
        Level bottomLevel);
    /// <summary>
    /// Метод получения плит по документам
    /// </summary>    
    /// <remarks>
    /// В данном методе производится получение всех плит перекрытий по документам
    /// </remarks>
    /// <returns>IEnumerable SlabElement</returns>
    IEnumerable<SlabElement> GetSlabsByDocs(IEnumerable<Document> documents);
}
