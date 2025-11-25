using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface IDocumentsService {
    /// <summary>
    /// Метод получения документа
    /// </summary>    
    /// <remarks>
    /// В данном методе производится получение документа по его имени
    /// </remarks>
    /// <returns>Document</returns>
    Document GetDocumentByName(string name);
    /// <summary>
    /// Метод получения документов
    /// </summary>    
    /// <remarks>
    /// В данном методе производится получение всех документов. Как связанных так и текущего
    /// </remarks>
    /// <returns>IEnumerable Document</returns>
    IEnumerable<Document> GetAllDocuments();
}
