using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface ISlabsService {
    /// <summary>
    /// Метод получения плиты
    /// </summary>    
    /// <remarks>
    /// В данном методе производится получения плиты перекрытия по имени
    /// </remarks>
    /// <returns>SlabElement</returns>
    IEnumerable<SlabElement> GetSlabsByTypesAndDocs(IEnumerable<string> name, IEnumerable<Document> documents);
    /// <summary>
    /// Метод получения плит по имени документа
    /// </summary>    
    /// <remarks>
    /// В данном методе производится получение всех плит перекрытий по имени документа, в котором они находятся
    /// </remarks>
    /// <returns>IEnumerable SlabElement</returns>
    IEnumerable<SlabElement> GetSlabsByDocs(IEnumerable<Document> documents);
}
