using System;
using System.Threading;

using RevitServerFolders.Models;

namespace RevitServerFolders.Services;
/// <summary>
/// Сервис для экспорта файлов
/// </summary>
internal interface IModelsExportService<T> where T : ExportSettings {
    /// <summary>
    /// Экспортирует заданные файлы в заданную директорию
    /// </summary>
    /// <param name="modelFiles">Массив абсолютных путей к файлам, которые нужно экспортировать</param>
    /// <param name="settings">Настройки экспорта файлов</param>
    /// <param name="progress">Прогресс для уведомления о ходе выполнения операции</param>
    /// <param name="ct">Токен отмены</param>
    void ExportModelObjects(
        string[] modelFiles,
        T settings,
        IProgress<int> progress = null,
        CancellationToken ct = default,
        int startProgress = 0);
}
