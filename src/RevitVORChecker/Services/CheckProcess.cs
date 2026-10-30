using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

using dosymep.SimpleServices;

using RevitVORChecker.Core;
using RevitVORChecker.Models;
using RevitVORChecker.Models.Checks;
using RevitVORChecker.Models.Data;
using RevitVORChecker.Models.Excel;
using RevitVORChecker.Models.Findings;
using RevitVORChecker.Models.Report;
using RevitVORChecker.Models.Revit;

namespace RevitVORChecker.Services;

/// <summary>
/// Полный цикл проверки папки с моделями: выгрузка моделей → спецификации сводной модели → правила →
/// сверка Excel → скриншоты → реестр .docx в «&lt;папка&gt;\Локальная проверка».
/// </summary>
/// <remarks>
/// Модели открываются в фоне отсоединённо и закрываются без сохранения; файлы моделей не изменяются.
/// </remarks>
internal class CheckProcess {
    private const int _screenshotsPerBatch = 10;

    /// <summary>
    /// Суффикс имени отсоединённой копии в русском и английском Revit.
    /// </summary>
    private static readonly string[] _detachedSuffixes = { "_отсоединено", "_detached" };

    private static readonly string[] _scheduleNames = {
        ExcelComparison.VorSchedule, ExcelComparison.InsulationSchedule, ExcelComparison.DirectCalculationSchedule
    };

    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    public CheckProcess(RevitRepository revitRepository, ILocalizationService localizationService) {
        _revitRepository = revitRepository;
        _localizationService = localizationService;
    }

    /// <summary>
    /// Число шагов прогресса для папки с <paramref name="modelCount"/> моделями: выгрузка каждой модели,
    /// спецификации, правила, скриншоты каждой модели (если включены), реестр.
    /// </summary>
    public static int GetStepCount(int modelCount, bool takeScreenshots) {
        return modelCount * (takeScreenshots ? 2 : 1) + 3;
    }

    /// <summary>
    /// Выполняет проверку. Возвращает путь реестра; при ошибке бросает исключение с понятным текстом.
    /// </summary>
    /// <param name="settings">Параметры запуска.</param>
    /// <param name="progress">Номер выполненного шага (из <see cref="GetStepCount"/>).</param>
    /// <param name="reportStatus">Текущее действие (записи журнала).</param>
    /// <param name="cancellationToken">Отмена между шагами.</param>
    public CheckResult Run(
        CheckSettings settings,
        IProgress<int> progress,
        Action<string> reportStatus,
        CancellationToken cancellationToken) {

        string folder = Path.GetFullPath(settings.ModelsFolder);
        if(!Directory.Exists(folder)) {
            throw new InvalidOperationException(GetText("Process.NoFolder") + " " + folder);
        }

        DateTime startTime = DateTime.Now;
        string outputFolder = ModelsFolder.GetOutputFolder(folder);
        Directory.CreateDirectory(outputFolder);
        var log = new CheckLog(
            Path.Combine(outputFolder, "Журнал проверки_" + startTime.ToString("yyyy-MM-dd_HHmm") + ".txt"),
            reportStatus);
        string workFolder = Path.Combine(
            Path.GetTempPath(),
            "ВОР_КР",
            // с секундами: повторный запуск в ту же минуту не должен взять скриншоты прошлого запуска
            Path.GetFileName(folder.TrimEnd('\\', '/')) + "_" + startTime.ToString("yyyyMMdd_HHmmss"));
        string imageFolder = Path.Combine(workFolder, "img");
        Directory.CreateDirectory(imageFolder);
        log.Write("Папка: " + folder);
        log.Write("Данные проекта: " + settings.Project.GetSummary());
        log.Write("Рабочая папка: " + workFolder);

        OrderedMap<string, string> models = ModelsFolder.GetModels(folder);
        if(models.Count == 0) {
            throw new InvalidOperationException(GetText("Process.NoModels"));
        }

        log.Write("Моделей: " + models.Count + " — " + string.Join(", ", models.Keys));
        string? classifierPath = ModelsFolder.FindClassifier(folder, settings.ClassifierPath, settings.Project);
        if(classifierPath is not { Length: > 0 } || !File.Exists(classifierPath)) {
            throw new InvalidOperationException(GetText("Process.NoClassifier"));
        }

        log.Write("Классификатор: " + classifierPath);
        EnsureModelsAreClosed(folder, models);

        int step = 0;
        void NextStep() {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(++step);
        }

        // Revit при построении скриншотов иногда показывает окно «серьёзная ошибка» (generateViewSpecificGRep):
        // отвечаем «Нет» — Revit продолжает работу, а кадр повторяется в упрощённом виде. Только на время проверки.
        int autoAnsweredDialogs = 0;
        void AnswerSeriousError(object sender, DialogBoxShowingEventArgs args) {
            if(args is TaskDialogShowingEventArgs taskDialog
               && taskDialog.DialogId == "TaskDialog_Serious_Error_Occurred_Recoverable") {
                taskDialog.OverrideResult((int) TaskDialogResult.No);
                autoAnsweredDialogs++;
            }
        }

        UIApplication uiApplication = _revitRepository.UIApplication;
        uiApplication.DialogBoxShowing += AnswerSeriousError;
        var openedTitles = new HashSet<string>(GetOpenDocuments().Select(document => document.Title));
        try {
            ExportModels(models, settings, workFolder, log, NextStep);
            string schedulesFile = ReadSchedules(models, workFolder, log);
            NextStep();

            List<Finding> findings = CheckModels(models, settings, workFolder, classifierPath, schedulesFile, folder, log);
            NextStep();

            if(settings.TakeScreenshots) {
                TakeScreenshots(findings, models, workFolder, imageFolder, log, NextStep, cancellationToken);
            }

            JsonFiles.Save(Path.Combine(workFolder, "findings.json"), new { findings });
            string cipher = Regex.Split(models.Keys.First(), "_KR")[0];
            string registryPath = Path.Combine(
                outputFolder,
                "Реестр замечаний_ВОР КР_" + cipher + "_" + startTime.ToString("yyyy-MM-dd") + ".docx");
            log.Write("Сборка реестра");
            RegistryResult registry = RegistryDocumentBuilder.Build(
                findings,
                registryPath,
                string.IsNullOrEmpty(settings.Project.Name) ? cipher : settings.Project.Name,
                string.Join(", ", models.Keys));
            NextStep();
            log.Write("   " + registry.Path + " | замечаний: " + findings.Count + " | рисунков: " + registry.ImageCount);
            if(autoAnsweredDialogs > 0) {
                log.Write("Окон «серьёзная ошибка Revit» закрыто автоматически («Нет»): " + autoAnsweredDialogs);
            }

            return new CheckResult(registry.Path, findings.Count, log.Path);
        } catch(Exception ex) {
            log.Write("ОСТАНОВЛЕНО: " + ex.Message);
            throw;
        } finally {
            uiApplication.DialogBoxShowing -= AnswerSeriousError;
            CloseOpenedCopies(openedTitles, log);
        }
    }

    private string GetText(string key) {
        return _localizationService.GetLocalizedString(key);
    }

    private IEnumerable<Document> GetOpenDocuments() {
        return _revitRepository.Application.Documents.Cast<Document>().Where(document => !document.IsLinked);
    }

    /// <summary>
    /// Модели из проверяемой папки должны быть закрыты: проверка открывает их сама, отсоединённо.
    /// </summary>
    private void EnsureModelsAreClosed(string folder, OrderedMap<string, string> models) {
        var openModels = GetOpenDocuments()
            .Where(document => (!string.IsNullOrEmpty(document.PathName)
                                && string.Equals(Path.GetDirectoryName(document.PathName), folder, StringComparison.OrdinalIgnoreCase))
                               || _detachedSuffixes.Any(suffix => document.Title.EndsWith(suffix)
                                   && models.ContainsKey(document.Title.Substring(0, document.Title.Length - suffix.Length))))
            .Select(document => document.Title)
            .ToList();
        if(openModels.Count > 0) {
            throw new InvalidOperationException(
                string.Format(GetText("Process.ModelsOpen"), string.Join(", ", openModels)));
        }
    }

    /// <summary>
    /// Выгрузка данных, пересечений и связей каждой модели в рабочую папку.
    /// </summary>
    private void ExportModels(
        OrderedMap<string, string> models,
        CheckSettings settings,
        string workFolder,
        CheckLog log,
        Action nextStep) {

        int modelNumber = 0;
        foreach(var model in models) {
            modelNumber++;
            log.Write("[" + modelNumber + "/" + models.Count + "] Выгрузка " + model.Key);
            Document? document = null;
            try {
                document = DetachedModelOpener.Open(_revitRepository.Application, model.Value, out List<LinkData> links);
                JsonFiles.Save(Path.Combine(workFolder, "links_" + model.Key + ".json"), links);
                ModelData modelData = ModelExporter.Export(document);
                JsonFiles.Save(Path.Combine(workFolder, "model_" + model.Key + ".json"), modelData);
                log.Write("   OK: элементов " + modelData.Elements.Count + ", удалено связей RVT " + links.Count);
                ClashesData clashes = ClashDetector.Detect(document, settings.ClashToleranceMm);
                JsonFiles.Save(Path.Combine(workFolder, "clashes_" + model.Key + ".json"), clashes);
                log.Write("   пересечений " + clashes.Clashes.Count + ", дублей " + clashes.Duplicates.Count
                          + ", ошибок геометрии " + clashes.GeometryErrorCount + ", " + clashes.Seconds + " с");
            } catch(OperationCanceledException) {
                throw;
            } catch(Exception ex) {
                log.Write("   ОШИБКА: " + ex.Message);
            } finally {
                document?.Close(false);
            }

            nextStep();
        }
    }

    /// <summary>
    /// Спецификации ВОР сводной модели (первая модель «_PRK») вместе с остальными моделями папки.
    /// </summary>
    private string ReadSchedules(OrderedMap<string, string> models, string workFolder, CheckLog log) {
        string schedulesFile = Path.Combine(workFolder, "schedules.json");
        string mainModel = models.Keys
            .Where(name => name.ToUpperInvariant().Contains("_PRK"))
            .OrderBy(name => name, StringComparer.Ordinal)
            .FirstOrDefault();
        if(mainModel == null) {
            log.Write("Нет модели _PRK — сверка Excel с моделью не выполняется");
            return schedulesFile;
        }

        log.Write("Сводная модель: " + mainModel + " — чтение спецификаций ВОР со связями");
        Document? document = null;
        try {
            document = DetachedModelOpener.Open(_revitRepository.Application, models[mainModel], out _);
            var linkPaths = models.Where(model => model.Key != mainModel).Select(model => model.Value).ToList();
            ScheduleTablesData schedules = ScheduleTableReader.Read(document, _scheduleNames, linkPaths);
            JsonFiles.Save(schedulesFile, schedules);
            foreach(string line in schedules.Log.Where(line => line.StartsWith("ОШИБКА"))) {
                log.Write("   " + line);
            }
        } catch(Exception ex) {
            log.Write("   сводная модель не прочитана — сверка Excel будет без спецификаций: " + ex.Message);
        } finally {
            document?.Close(false);
        }

        return schedulesFile;
    }

    private static List<Finding> CheckModels(
        OrderedMap<string, string> models,
        CheckSettings settings,
        string workFolder,
        string classifierPath,
        string schedulesFile,
        string folder,
        CheckLog log) {

        var modelFiles = GetWorkFiles(models, workFolder, "model_");
        var clashFiles = GetWorkFiles(models, workFolder, "clashes_");
        var notExported = models.Keys.Where(name => !File.Exists(Path.Combine(workFolder, "model_" + name + ".json"))).ToList();
        if(notExported.Count > 0) {
            log.Write("ВНИМАНИЕ: не выгружены модели: " + string.Join(", ", notExported) + " — они не проверены.");
        }

        log.Write("Проверка правил");
        if(!settings.CheckWorksets) {
            log.Write("Проверка распределения элементов ВОР по рабочим наборам отключена.");
        }

        List<Finding> findings = ModelChecker.Check(
            modelFiles,
            clashFiles,
            new WorkClassifier(classifierPath),
            settings.Project,
            settings.CheckWorksets);
        var countsBySection = ReportSections.Titles.Keys
            .Select(section => section + "=" + findings.Count(finding => finding.Section == section))
            .Where(count => !count.EndsWith("=0"));
        log.Write("   замечаний: " + findings.Count + " | " + string.Join(", ", countsBySection));

        log.Write("Сверка Excel");
        List<Finding> excelFindings = ExcelComparison.Compare(
            File.Exists(schedulesFile) ? schedulesFile : "",
            folder,
            modelFiles,
            settings.Project);
        log.Write("   записей: " + excelFindings.Count);
        return findings.Concat(excelFindings).ToList();
    }

    private static List<string> GetWorkFiles(OrderedMap<string, string> models, string workFolder, string prefix) {
        return models.Keys
            .Select(name => Path.Combine(workFolder, prefix + name + ".json"))
            .Where(File.Exists)
            .ToList();
    }

    /// <summary>
    /// Скриншоты по моделям: пачками по 10, затем одна повторная попытка для не снятых.
    /// </summary>
    private void TakeScreenshots(
        List<Finding> findings,
        OrderedMap<string, string> models,
        string workFolder,
        string imageFolder,
        CheckLog log,
        Action nextStep,
        CancellationToken cancellationToken) {

        var modelFiles = GetWorkFiles(models, workFolder, "model_");
        OrderedMap<string, List<ScreenshotJob>> jobsByModel = ScreenshotPlanner.Plan(findings, imageFolder, modelFiles);
        bool IsMissing(ScreenshotJob job) {
            return !File.Exists(Path.Combine(imageFolder, job.Name + ".png"));
        }

        // на скриншоты отведено по шагу на модель (GetStepCount): шаг — после первой попытки по модели
        int screenshotSteps = 0;
        void NextScreenshotStep() {
            if(screenshotSteps < models.Count) {
                screenshotSteps++;
                nextStep();
            }
        }

        for(int attempt = 1; attempt <= 2; attempt++) {
            var pending = jobsByModel
                .Select(model => (Model: model.Key, Jobs: model.Value.Where(IsMissing).ToList()))
                .Where(model => model.Jobs.Count > 0)
                .ToList();
            foreach((string modelName, List<ScreenshotJob> jobs) in pending) {
                string modelPath = FindModelPath(models, modelName);
                if(modelPath != null) {
                    log.Write("Скриншоты " + modelName + ": " + jobs.Count + " шт." + (attempt > 1 ? " (повтор)" : ""));
                    for(int start = 0; start < jobs.Count; start += _screenshotsPerBatch) {
                        cancellationToken.ThrowIfCancellationRequested();
                        TakeScreenshotBatch(modelPath, imageFolder, jobs.Skip(start).Take(_screenshotsPerBatch), log);
                    }
                } else {
                    log.Write("   нет файла модели для " + modelName);
                }

                if(attempt == 1) {
                    NextScreenshotStep();
                }
            }
        }

        while(screenshotSteps < models.Count) {
            NextScreenshotStep();
        }

        var lostImages = jobsByModel.Values.SelectMany(jobs => jobs).Where(IsMissing).Select(job => job.Name).ToList();
        if(lostImages.Count > 0) {
            log.Write("Не удалось снять " + lostImages.Count + " скриншотов: " + string.Join(", ", lostImages.Take(20)));
        }
    }

    private void TakeScreenshotBatch(string modelPath, string imageFolder, IEnumerable<ScreenshotJob> jobs, CheckLog log) {
        Document? document = null;
        try {
            document = DetachedModelOpener.Open(_revitRepository.Application, modelPath, out _);
            foreach(string line in ScreenshotMaker.Take(document, imageFolder, jobs).Where(line => line.Contains("ОШИБКА"))) {
                log.Write("   " + line);
            }
        } catch(Exception ex) {
            log.Write("   ОШИБКА скриншотов: " + ex.Message);
        } finally {
            document?.Close(false);
        }
    }

    private static string FindModelPath(OrderedMap<string, string> models, string modelName) {
        if(models.TryGetValue(modelName, out string path)) {
            return path;
        }

        return models.FirstOrDefault(model => modelName.StartsWith(model.Key) || model.Key.StartsWith(modelName)).Value;
    }

    /// <summary>
    /// Закрывает копии моделей, которые остались открытыми после ошибки: все документы, открытые за время проверки
    /// (у моделей без совместной работы и в английском Revit нет суффикса «_отсоединено»).
    /// </summary>
    private void CloseOpenedCopies(HashSet<string> openedTitles, CheckLog log) {
        var copies = GetOpenDocuments()
            .Where(document => !openedTitles.Contains(document.Title))
            .ToList();
        foreach(Document document in copies) {
            string title = document.Title;
            try {
                document.Close(false);
                log.Write("Закрыта копия: " + title);
            } catch(Exception ex) {
                log.Write("Не закрыта копия " + title + ": " + ex.Message);
            }
        }
    }
}

/// <summary>
/// Итог проверки.
/// </summary>
internal class CheckResult {
    public CheckResult(string registryPath, int findingCount, string logPath) {
        RegistryPath = registryPath;
        FindingCount = findingCount;
        LogPath = logPath;
    }

    public string RegistryPath { get; }

    public int FindingCount { get; }

    public string LogPath { get; }
}
