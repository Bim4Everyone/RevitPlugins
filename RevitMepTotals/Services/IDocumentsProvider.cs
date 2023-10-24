using System.Collections.Generic;

using RevitMepTotals.Models.Interfaces;

namespace RevitMepTotals.Services {
    /// <summary>
    /// Сервис, предоставляющий методы по выбору документов моделей Revit
    /// </summary>
    interface IDocumentsProvider {
        /// <summary>
        /// Возвращает коллекцию моделей Revit, выбранных пользователем
        /// </summary>
        /// <returns></returns>
        ICollection<IDocument> GetDocuments();
    }
}
