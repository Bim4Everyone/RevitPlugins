using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface IDocumentService {
    /// <summary>
    /// Метод получения документа.
    /// </summary>
    /// <remarks>
    /// В данном методе производится получение документа по его имени.
    /// </remarks>
    /// <param name="name">Имя искомого документа.</param>   
    /// <returns>
    /// Document.
    /// </returns>
    Document GetDocumentByName(string name);
    /// <summary>
    /// Метод получения трансформации.
    /// </summary>
    /// <remarks>
    /// В данном методе производится получение трансформации по имени документа.
    /// </remarks>
    /// <param name="name">Имя искомого документа для получения трансформации.</param>   
    /// <returns>
    /// Transform.
    /// </returns>
    Transform GetTransformByName(string name);
    /// <summary>
    /// Метод получения всех документов.
    /// </summary>
    /// <remarks>
    /// В данном методе производится получение всех документов. Как связанных так и текущего.
    /// </remarks>       
    /// <returns>
    /// Список документов.
    /// </returns> 
    IEnumerable<Document> GetAllDocuments();
}
