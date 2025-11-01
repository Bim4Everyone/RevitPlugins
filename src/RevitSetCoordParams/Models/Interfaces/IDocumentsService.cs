using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSetCoordParams.Models.Interfaces;
internal interface IDocumentsService {
    /// <summary>
    /// Метод получения документа
    /// </summary>    
    /// <remarks>
    /// В данном методе производится получение документа по имени
    /// </remarks>
    /// <returns>Document</returns>
    Document GetDocumentByNamePart(string namePart);
    /// <summary>
    /// Метод получения трансформации
    /// </summary>    
    /// <remarks>
    /// В данном методе производится получение трансформации по имени документа
    /// </remarks>
    /// <returns>Transform</returns>
    Transform GetTransformByName(string name);
    /// <summary>
    /// Метод получения документов
    /// </summary>    
    /// <remarks>
    /// В данном методе производится получение всех документов. Как связанных так и текущего
    /// </remarks>
    /// <returns>IEnumerable Document</returns>
    IEnumerable<Document> GetAllDocuments();
}
