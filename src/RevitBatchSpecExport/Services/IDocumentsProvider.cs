using System.Collections.Generic;

using RevitBatchSpecExport.Models.Interfaces;

namespace RevitBatchSpecExport.Services;
/// <summary>
/// Сервис, предоставляющий методы по выбору документов моделей Revit
/// </summary>
internal interface IDocumentsProvider {
    /// <summary>
    /// Возвращает коллекцию моделей Revit, выбранных пользователем
    /// </summary>
    /// <returns></returns>
    ICollection<IDocument> GetDocuments();
}
