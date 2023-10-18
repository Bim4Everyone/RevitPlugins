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
        /// <param name="directory"></param>
        /// <param name="documentData"></param>
        void ExportData(DirectoryInfo directory, IList<IDocumentData> documentData);
    }
}
