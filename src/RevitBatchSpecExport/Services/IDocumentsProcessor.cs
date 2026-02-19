using System;
using System.Collections.Generic;
using System.Threading;

using RevitBatchSpecExport.Models.Interfaces;

namespace RevitBatchSpecExport.Services;
/// <summary>
/// Сервис для обработки документов Revit
/// </summary>
internal interface IDocumentsProcessor {
    /// <summary>
    /// Обрабатывает документы Revit
    /// </summary>
    /// <param name="documents">Коллекция документов для обработки</param>
    /// <param name="errorMessage">Сообщение об ошибках, возникших в ходе обработки документов</param>
    /// <param name="progress">Прогресс бар</param>
    /// <param name="ct">Токен отмены операции</param>
    /// <returns>Обработанные данные</returns>
    IList<IDocumentData> ProcessDocuments(
        ICollection<IDocument> documents,
        out string errorMessage,
        IProgress<int> progress = null,
        CancellationToken ct = default);
}
