using System.Collections.Generic;
using System.IO;

using RevitMepTotals.Models.Interfaces;

namespace RevitMepTotals.Services {
    /// <summary>
    /// Сервис, экспортирующий данные
    /// </summary>
    internal interface IDataExporter {
        /// <summary>
        /// Экспортирует заданные данные в заданную директорию
        /// </summary>
        /// <param name="directory">Директория для сохранения данных</param>
        /// <param name="documentData">Данные для экспорта</param>
        /// <param name="error">Сообщение об ошибках, возникших при экспорте</param>
        void ExportData(DirectoryInfo directory, IList<IDocumentData> documentData, out string error);
    }
}
