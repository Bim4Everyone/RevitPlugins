using System.IO;

using Autodesk.Revit.DB;

using RevitBatchSpecExport.Models;

namespace RevitBatchSpecExport.Services;

/// <summary>
/// Сервис, экспортирующий данные
/// </summary>
internal interface IDataExporter {
    /// <summary>
    /// Экспортирует данные документа в заданную директорию
    /// </summary>
    /// <param name="docExportDirectory">Директория для сохранения данных документа</param>
    /// <param name="document">Документ для экспорта</param>
    /// <param name="config">Настройки экспорта</param>
    void ExportData(DirectoryInfo docExportDirectory, Document document, PluginConfig config);
}
