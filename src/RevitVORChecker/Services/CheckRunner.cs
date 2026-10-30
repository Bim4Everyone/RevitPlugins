using System.IO;

using dosymep.SimpleServices;

using RevitVORChecker.Models;

namespace RevitVORChecker.Services;

/// <summary>
/// Запуск проверки после закрытия окна параметров: окно хода проверки и итог проверки.
/// </summary>
/// <remarks>
/// Итог показывает команда: сообщения — только через сервис сообщений, а окно хода проверки
/// к этому моменту закрыто (оно поверх остальных окон).
/// </remarks>
internal class CheckRunner {
    private readonly CheckProcess _checkProcess;
    private readonly ILocalizationService _localizationService;

    public CheckRunner(CheckProcess checkProcess, ILocalizationService localizationService) {
        _checkProcess = checkProcess;
        _localizationService = localizationService;
    }

    public CheckOutcome Run(CheckSettings settings) {
        try {
            int stepCount = CheckProcess.GetStepCount(
                ModelsFolder.GetModels(settings.ModelsFolder).Count,
                settings.TakeScreenshots);

            using CheckProgressHost progressHost = CheckProgressHost.Show(
                GetText("MainWindow.Title"),
                GetText("Progress.Step"),
                GetText("Progress.Cancel"),
                GetText("Progress.Cancelling"),
                stepCount);
            return CheckOutcome.Completed(_checkProcess.Run(
                settings,
                progressHost,
                progressHost.ReportStatus,
                progressHost.CancellationToken));
        } catch(OperationCanceledException) {
            return CheckOutcome.Cancelled();
        } catch(Exception ex) {
            return CheckOutcome.Failed(ex, WriteErrorReport(settings.ModelsFolder, ex));
        }
    }

    private string GetText(string key) {
        return _localizationService.GetLocalizedString(key);
    }

    /// <summary>
    /// Полный текст ошибки в «&lt;папка&gt;\Локальная проверка». Возвращает путь файла или null.
    /// </summary>
    private static string? WriteErrorReport(string modelsFolder, Exception exception) {
        try {
            string folder = ModelsFolder.GetOutputFolder(modelsFolder);
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, "Ошибка проверки_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + ".txt");
            File.WriteAllText(path, exception.ToString());
            return path;
        } catch(IOException) {
            return null;
        } catch(UnauthorizedAccessException) {
            return null;
        }
    }
}

/// <summary>
/// Итог запуска проверки: выполнена, отменена или остановлена ошибкой.
/// </summary>
internal class CheckOutcome {
    private CheckOutcome() {
    }

    /// <summary>
    /// Итог выполненной проверки; null — проверка отменена или остановлена.
    /// </summary>
    public CheckResult? Result { get; private set; }

    public bool IsCancelled { get; private set; }

    /// <summary>
    /// Ошибка, остановившая проверку; null — ошибки не было.
    /// </summary>
    public Exception? Error { get; private set; }

    /// <summary>
    /// Файл с полным текстом ошибки; null — не записан.
    /// </summary>
    public string? ErrorReportPath { get; private set; }

    public static CheckOutcome Completed(CheckResult result) {
        return new CheckOutcome { Result = result };
    }

    public static CheckOutcome Cancelled() {
        return new CheckOutcome { IsCancelled = true };
    }

    public static CheckOutcome Failed(Exception error, string? errorReportPath) {
        return new CheckOutcome { Error = error, ErrorReportPath = errorReportPath };
    }
}
