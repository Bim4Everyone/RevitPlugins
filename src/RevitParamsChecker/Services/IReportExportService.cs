using dosymep.SimpleServices;

using RevitParamsChecker.ViewModels.Results;

namespace RevitParamsChecker.Services;

/// <summary>
/// Экспорт отчета по результату проверки во внешний файл
/// </summary>
internal interface IReportExportService {
    /// <summary>
    /// Показывает диалог сохранения и записывает отчет в выбранный файл.
    /// Если пользователь отменил диалог, ничего не делает.
    /// </summary>
    /// <param name="saveFileDialogService">Сервис диалога сохранения файла</param>
    /// <param name="checkResult">Результат проверки. Читается синхронно, в потоке ui</param>
    void Export(ISaveFileDialogService saveFileDialogService, CheckResultViewModel checkResult);
}
