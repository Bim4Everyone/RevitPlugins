using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface ISlabService {
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
    /// В данном методе производится получения плит перекрытий по их именам типов, документам и уровням
    /// </remarks>
    /// <returns>IEnumerable SlabElement</returns>
    IEnumerable<SlabElement> GetSlabsByTypesDocsAndLevels(
        IEnumerable<string> typeSlabs, IEnumerable<Document> documents, List<Level> levels);
    /// <summary>
    /// Метод получения плит по документам
    /// </summary>    
    /// <remarks>
    /// В данном методе производится получение всех плит перекрытий по документам
    /// </remarks>
    /// <returns>IEnumerable SlabElement</returns>
    IEnumerable<SlabElement> GetSlabsByDocs(IEnumerable<Document> documents);
}
