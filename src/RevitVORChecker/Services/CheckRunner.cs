using System.Diagnostics;
using System.IO;

using Autodesk.Revit.UI;

using dosymep.SimpleServices;

using RevitVORChecker.Models;

namespace RevitVORChecker.Services;

/// <summary>
/// Запуск проверки после закрытия окна параметров: окно хода проверки, итог и ошибки — в окне Revit.
/// </summary>
internal class CheckRunner {
    private readonly CheckProcess _checkProcess;
    private readonly ILocalizationService _localizationService;

    public CheckRunner(CheckProcess checkProcess, ILocalizationService localizationService) {
        _checkProcess = checkProcess;
        _localizationService = localizationService;
    }

    public void Run(CheckSettings settings) {
        string title = GetText("MainWindow.Title");
        CheckResult result = null;
        Exception error = null;
        try {
            int stepCount = CheckProcess.GetStepCount(
                ModelsFolder.GetModels(settings.ModelsFolder).Count,
                settings.TakeScreenshots);

            // итог и ошибки показываются после закрытия окна хода проверки: оно поверх остальных окон
            using CheckProgressHost progressHost = CheckProgressHost.Show(
                title,
                GetText("Progress.Step"),
                GetText("Progress.Cancel"),
                GetText("Progress.Cancelling"),
                stepCount);
            result = _checkProcess.Run(
                settings,
                progressHost,
                progressHost.ReportStatus,
                progressHost.CancellationToken);
        } catch(Exception ex) {
            error = ex;
        }

        if(error is OperationCanceledException) {
            TaskDialog.Show(title, GetText("MainWindow.Cancelled"));
        } else if(error != null) {
            ShowError(title, settings, error);
        } else {
            ShowResult(title, result);
        }
    }

    private string GetText(string key) {
        return _localizationService.GetLocalizedString(key);
    }

    private void ShowResult(string title, CheckResult result) {
        var dialog = new TaskDialog(title) {
            MainInstruction = string.Format(GetText("Result.Instruction"), result.FindingCount),
            MainContent = GetText("Result.Registry") + "\n" + result.RegistryPath
        };
        dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, GetText("Result.OpenFolder"));
        dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, GetText("Result.Close"));
        if(dialog.Show() == TaskDialogResult.CommandLink1) {
            Process.Start("explorer.exe", "/select,\"" + result.RegistryPath + "\"");
        }
    }

    private void ShowError(string title, CheckSettings settings, Exception exception) {
        string reportPath = WriteErrorReport(settings.ModelsFolder, exception);
        string details = reportPath != null ? "\n\n" + GetText("MainWindow.ErrorReport") + "\n" + reportPath : "";
        var dialog = new TaskDialog(title) {
            MainIcon = TaskDialogIcon.TaskDialogIconError,
            MainInstruction = GetText("MainWindow.Stopped"),
            MainContent = exception.GetBaseException().Message + details
        };
        dialog.Show();
    }

    /// <summary>
    /// Полный текст ошибки в «&lt;папка&gt;\Локальная проверка». Возвращает путь файла или null.
    /// </summary>
    private static string WriteErrorReport(string modelsFolder, Exception exception) {
        try {
            string folder = ModelsFolder.GetOutputFolder(modelsFolder);
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, "Ошибка проверки_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + ".txt");
            File.WriteAllText(path, exception.ToString());
            return path;
        } catch(Exception) {
            return null;
        }
    }
}
