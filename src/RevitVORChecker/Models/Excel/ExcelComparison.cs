using System.IO;
using System.Text.RegularExpressions;

using RevitVORChecker.Core;
using RevitVORChecker.Models.Data;
using RevitVORChecker.Models.Findings;
using RevitVORChecker.Models.Project;

namespace RevitVORChecker.Models.Excel;

/// <summary>
/// Сверка выгрузок ВОР (Excel) со спецификациями сводной модели, целостность объёмов ВОР, вентблоки и УЭРВ.
/// </summary>
internal static class ExcelComparison {
    public const string VorSchedule = "ВОР_МТР_ТИП_КР";
    public const string InsulationSchedule = "ВОР_МТР_ТИП_КР_Изоляция";
    public const string DirectCalculationSchedule = "КЖ_Изоляция_(+Прямой расчет)";

    /// <summary>
    /// Сверяет файлы Excel папки с моделями.
    /// </summary>
    /// <param name="schedulesFile">schedules.json со спецификациями сводной модели (может отсутствовать).</param>
    /// <param name="modelsFolder">Папка с моделями и выгрузками ВОР.</param>
    /// <param name="modelFiles">Файлы model_*.json.</param>
    /// <param name="project">Данные проекта.</param>
    public static List<Finding> Compare(
        string schedulesFile,
        string modelsFolder,
        IEnumerable<string> modelFiles,
        ProjectCard project) {

        var schedules = new OrderedMap<string, ScheduleTableData>();
        if(!string.IsNullOrEmpty(schedulesFile) && File.Exists(schedulesFile)) {
            foreach(ScheduleTableData schedule in JsonFiles.Load<ScheduleTablesData>(schedulesFile).Schedules) {
                schedules[schedule.Name] = schedule;
            }
        }

        List<ModelSummary> models = ModelSummary.Load(modelFiles);
        var findings = new List<Finding>();
        var comparisonWriter = new FindingWriter(
            findings,
            ReportSections.ExcelComparison,
            ReportSections.GetTitle(ReportSections.ExcelComparison));
        var integrityWriter = new FindingWriter(
            findings,
            ReportSections.Integrity,
            ReportSections.GetTitle(ReportSections.Integrity));
        var ventilationWriter = new FindingWriter(
            findings,
            ReportSections.Ventilation,
            ReportSections.GetTitle(ReportSections.Ventilation));

        if(schedules.Count == 0) {
            comparisonWriter.Add(
                "Спецификации сводной модели не прочитаны — сверка Excel с моделью не выполнялась.",
                FindingKinds.Clarify);
        }

        foreach(var schedule in schedules.Where(schedule => schedule.Value.IsMissing)) {
            comparisonWriter.Add("В сводной модели нет спецификации «" + schedule.Key + "».");
        }

        var fileNames = Directory.EnumerateFileSystemEntries(modelsFolder).Select(Path.GetFileName).ToList();
        List<string> FindFiles(string pattern) {
            return fileNames
                .Where(name => Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase) && !name.StartsWith("~$"))
                .Select(name => Path.Combine(modelsFolder, name))
                .ToList();
        }

        List<string> vorFiles = FindFiles(@"_ВОР\.xlsm$");
        List<string> insulationFiles = FindFiles(@"ВОР_Изоляция\.xlsx$");
        List<string> directCalculationFiles = FindFiles(@"Изоляция_\(\+Прямой расчет\)\.xlsx$");
        ReportMissingFiles(vorFiles, insulationFiles, directCalculationFiles, comparisonWriter);
        ReportUnknownFiles(fileNames, modelsFolder, vorFiles, insulationFiles, directCalculationFiles, comparisonWriter);

        ScheduleTableData FindSchedule(string name) {
            return schedules.TryGetValue(name, out ScheduleTableData schedule) && !schedule.IsMissing ? schedule : null;
        }

        // блоки всех файлов ВОР (по корпусам их может быть несколько) — для сверки вентблоков
        var vorRecordsByBlock = new OrderedMap<string, List<VorRecord>>();
        bool isVorScheduleCompared = false;
        foreach(string vorFile in vorFiles) {
            string fileName = Path.GetFileName(vorFile);
            TryProcessFile(fileName, comparisonWriter, () => {
                using var workbook = new XlsxReader(vorFile);
                OrderedMap<string, string> sheets = workbook.GetSheetPaths();
                string sourceSheet = sheets.Keys.FirstOrDefault(name => name.Trim().ToLowerInvariant() == "исходная вор");
                if(sourceSheet == null) {
                    comparisonWriter.Add(fileName + ": нет листа «Исходная ВОР».");
                    return;
                }

                List<SheetRow> rows = workbook.ReadSheet(sheets[sourceSheet]);
                List<VorRecord> records = VorRecord.ReadAll(rows);
                var fileRecordsByBlock = VolumeIntegrityChecker.Check(records, models, project, fileName, integrityWriter);
                foreach(var block in fileRecordsByBlock) {
                    vorRecordsByBlock.GetOrAdd(block.Key, () => new List<VorRecord>()).AddRange(block.Value);
                }

                if(FindSchedule(VorSchedule) != null) {
                    schedules.TryGetValue(InsulationSchedule, out ScheduleTableData insulation);
                    new VorScheduleComparer().Compare(schedules[VorSchedule], rows, fileName, comparisonWriter, insulation);
                    isVorScheduleCompared = true;
                }

                double total = 0;
                foreach(VorRecord record in records) {
                    total += ExcelText.ParseNumber(record.Get("Количество")) ?? 0;
                }

                PivotTableChecker.Check(workbook, sheets, sourceSheet, records.Count, total, fileName, comparisonWriter);
            });
        }

        if(isVorScheduleCompared) {
            ReportUnsortedModels(models, comparisonWriter);
        }

        foreach(string insulationFile in insulationFiles) {
            string fileName = Path.GetFileName(insulationFile);
            TryProcessFile(fileName, comparisonWriter, () => {
                using var workbook = new XlsxReader(insulationFile);
                if(FindSchedule(InsulationSchedule) != null) {
                    List<SheetRow> rows = workbook.ReadSheet(workbook.GetSheetPaths().Values.First());
                    new VorScheduleComparer().Compare(schedules[InsulationSchedule], rows, fileName, comparisonWriter);
                }
            });
        }

        foreach(string directCalculationFile in directCalculationFiles) {
            string fileName = Path.GetFileName(directCalculationFile);
            TryProcessFile(fileName, comparisonWriter, () => {
                using var workbook = new XlsxReader(directCalculationFile);
                if(FindSchedule(DirectCalculationSchedule) != null) {
                    List<SheetRow> rows = workbook.ReadSheet(workbook.GetSheetPaths().Values.First());
                    DirectCalculationComparer.Compare(schedules[DirectCalculationSchedule], rows, fileName, comparisonWriter);
                }
            });
        }

        VentilationChecker.Check(modelsFolder, fileNames, vorRecordsByBlock, ventilationWriter);
        return findings;
    }

    /// <summary>
    /// Сверка одного файла Excel: ошибка чтения или сверки файла — замечание, остальные файлы сверяются дальше.
    /// </summary>
    public static void TryProcessFile(string fileName, FindingWriter writer, Action process) {
        try {
            process();
        } catch(Exception ex) {
            writer.Add(
                fileName + ": файл не прочитан или не сверен (" + ex.GetBaseException().Message + "). Проверить, что это "
                + "книга .xlsx/.xlsm без пароля, открывается в Excel, и пересохранить её.",
                FindingKinds.Clarify);
        }
    }

    private static void ReportMissingFiles(
        List<string> vorFiles,
        List<string> insulationFiles,
        List<string> directCalculationFiles,
        FindingWriter writer) {

        var expectedFiles = new[] {
            (Files: vorFiles, Mask: "*_ВОР.xlsm"),
            (Files: insulationFiles, Mask: "*ВОР_Изоляция.xlsx"),
            (Files: directCalculationFiles, Mask: "*КЖ_Изоляция_(+Прямой расчет).xlsx")
        };
        foreach(var expected in expectedFiles.Where(expected => expected.Files.Count == 0)) {
            writer.Add(
                "В папке нет файла " + expected.Mask + " (стандарт имён: «Имя проекта_ВОР.xlsm», «…ВОР_Изоляция.xlsx», "
                + "«…КЖ_Изоляция_(+Прямой расчет).xlsx»).");
        }
    }

    private static void ReportUnknownFiles(
        List<string> fileNames,
        string modelsFolder,
        List<string> vorFiles,
        List<string> insulationFiles,
        List<string> directCalculationFiles,
        FindingWriter writer) {

        var ventilationFiles = fileNames.Where(name => Regex.IsMatch(name, "вент[а]?блок|уэрв", RegexOptions.IgnoreCase)).ToList();
        var knownFiles = new HashSet<string>(vorFiles.Concat(insulationFiles).Concat(directCalculationFiles));
        var unknownFiles = fileNames
            .Where(name => IsExcelFile(name)
                           && !name.StartsWith("~$")
                           && !knownFiles.Contains(Path.Combine(modelsFolder, name))
                           && !ventilationFiles.Contains(name)
                           && !Regex.IsMatch(name, "классификатор", RegexOptions.IgnoreCase))
            .ToList();
        if(unknownFiles.Count > 0) {
            writer.Add(
                "Файлы Excel с нестандартными именами (не проверялись): " + string.Join(", ", unknownFiles) + ".",
                FindingKinds.Clarify);
        }
    }

    private static bool IsExcelFile(string name) {
        string lowerName = name.ToLowerInvariant();
        return lowerName.EndsWith(".xlsx") || lowerName.EndsWith(".xlsm") || lowerName.EndsWith(".xls");
    }

    /// <summary>
    /// Вероятная причина расхождений: у большинства элементов ВОР не заполнена ФОП_ТИП_Доп. сортировка.
    /// </summary>
    private static void ReportUnsortedModels(List<ModelSummary> models, FindingWriter writer) {
        var unsortedModels = models
            .Where(model => model.VorElementCount >= 20 && model.UnsortedElementCount > model.VorElementCount / 2.0)
            .Select(model => model.FileTitle.Replace("_отсоединено", "") + " (" + model.UnsortedElementCount + " из "
                             + model.VorElementCount + ")")
            .ToList();
        if(unsortedModels.Count == 0) {
            return;
        }

        writer.Add(
            "Вероятная причина расхождений: спецификации ВОР группируют строки по «ФОП_ТИП_Доп. сортировка» "
            + "(без «Для каждого экземпляра»). В моделях " + string.Join(", ", unsortedModels) + " у большинства "
            + "элементов ВОР этот параметр пустой — их строки в спецификации схлопываются, поэтому количество и состав "
            + "строк не совпадают с Excel. Excel, по-видимому, выгружен при другом (заполненном) состоянии модели. "
            + "Заполнить параметр плагином «Перенос параметров» и выгрузить ВОР заново.",
            FindingKinds.Info);
    }
}
