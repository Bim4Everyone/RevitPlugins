using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface ISlabService {
    /// <summary>
    /// Метод получения перекрытий.
    /// </summary>
    /// <remarks>
    /// В данном методе производится получения плит перекрытий по их именам типов и документам.
    /// </remarks>
    /// <param name="typeSlabs">Коллекция имен типов перекрытий.</param>        
    /// <param name="documents">Коллекция документов.</param>        
    /// <returns>
    /// Коллекция SlabElement.
    /// </returns>
    IEnumerable<SlabElement> GetSlabsByTypesAndDocs(IEnumerable<string> typeSlabs, IEnumerable<Document> documents);
    /// <summary>
    /// Метод получения перекрытий.
    /// </summary>
    /// <remarks>
    /// В данном методе производится получения плит перекрытий по их именам типов, документам и уровням.
    /// </remarks>
    /// <param name="typeSlabs">Коллекция имен типов перекрытий.</param>        
    /// <param name="documents">Коллекция документов.</param>        
    /// <param name="levels">Уровни перекрытий.</param>        
    /// <returns>
    /// Коллекция SlabElement.
    /// </returns>
    IEnumerable<SlabElement> GetSlabsByTypesDocsAndLevels(
        IEnumerable<string> typeSlabs, IEnumerable<Document> documents, List<Level> levels);
    /// <summary>
    /// Метод получения перекрытий.
    /// </summary>
    /// <remarks>
    /// В данном методе производится получения плит перекрытий по документам.
    /// </remarks>            
    /// <param name="documents">Коллекция документов.</param>         
    /// <returns>
    /// Коллекция SlabElement.
    /// </returns>
    IEnumerable<SlabElement> GetSlabsByDocs(IEnumerable<Document> documents);
}
