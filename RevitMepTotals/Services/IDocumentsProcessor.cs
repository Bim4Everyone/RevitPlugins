using System.Collections.Generic;

using RevitMepTotals.Models.Interfaces;

namespace RevitMepTotals.Services {
    /// <summary>
    /// Сервис для обработки документов Revit
    /// </summary>
    interface IDocumentsProcessor {
        /// <summary>
        /// Обрабатывает документы Revit
        /// </summary>
        /// <param name="documents">Коллекция документов для обработки</param>
        void ProcessDocuments(ICollection<IDocument> documents);
    }
}
