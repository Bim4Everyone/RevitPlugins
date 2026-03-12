using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using RevitBatchSpecExport.Models;

namespace RevitBatchSpecExport.Services;

/// <summary>
/// Сервис для обработки документов Revit
/// </summary>
internal interface IDocumentsProcessor {
    /// <summary>
    /// Обрабатывает документы Revit
    /// </summary>
    /// <param name="rootExportDir">Директория, в которую будут сохраняться результаты экспорта</param>
    /// <param name="documents">Коллекция документов для обработки</param>
    /// <param name="config">Настройки экспорта</param>
    /// <param name="progress">Прогресс бар</param>
    /// <param name="ct">Токен отмены операции</param>
    /// <returns>Сообщение об ошибках, возникших в ходе обработки документов</returns>
    string ProcessDocuments(
        DirectoryInfo rootExportDir,
        ICollection<IDocument> documents,
        PluginConfig config,
        IProgress<int> progress = null,
        CancellationToken ct = default);
}
