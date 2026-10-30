using System.IO;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitVORChecker.Models.Data;

namespace RevitVORChecker.Models.Revit;

/// <summary>
/// Чтение спецификаций сводной модели вместе с остальными моделями папки — для сверки с выгрузками ВОР в Excel.
/// </summary>
/// <remarks>
/// Остальные модели временно подгружаются связями (по общим координатам), в спецификациях временно включается
/// «Включать элементы из связей». Все изменения делаются в транзакции, которая откатывается.
/// </remarks>
internal static class ScheduleTableReader {
    private const string _linksWorkset = "00_Связи_RVT_02_КР_П";

    public static ScheduleTablesData Read(Document document, IEnumerable<string> scheduleNames, IList<string> linkPaths) {
        var result = new ScheduleTablesData { FileTitle = document.Title };
        using var transaction = new Transaction(document, "Проверка ВОР: чтение спецификаций (откат)");
        transaction.Start();
        try {
            LoadLinks(document, linkPaths, result.Log);
            document.Regenerate();
            foreach(string scheduleName in scheduleNames) {
                result.Schedules.Add(ReadSchedule(document, scheduleName, result.Log));
            }
        } finally {
            transaction.RollBack();
        }

        return result;
    }

    private static void LoadLinks(Document document, IList<string> linkPaths, List<string> log) {
        var existingLinks = new FilteredElementCollector(document)
            .OfClass(typeof(RevitLinkType))
            .Cast<RevitLinkType>()
            .ToList();
        foreach(RevitLinkType link in existingLinks) {
            try {
                if(!RevitLinkType.IsLoaded(document, link.Id)) {
                    link.Load();
                    log.Add("загружена существующая связь: " + link.Name);
                }
            } catch(Exception ex) {
                log.Add("не удалось загрузить связь " + link.Name + ": " + ex.Message);
            }
        }

        WorksetId? previousWorkset = SelectLinksWorkset(document, linkPaths.Count > 0, log);
        foreach(string linkPath in linkPaths) {
            string fileName = Path.GetFileName(linkPath);
            if(string.Equals(linkPath, document.PathName, StringComparison.OrdinalIgnoreCase)) {
                continue;
            }

            string modelName = Path.GetFileNameWithoutExtension(linkPath);
            if(existingLinks.Any(link => link.Name.StartsWith(modelName, StringComparison.OrdinalIgnoreCase))) {
                log.Add("связь уже есть: " + fileName);
                continue;
            }

            CreateLink(document, linkPath, fileName, log);
        }

        if(previousWorkset != null) {
            try {
                document.GetWorksetTable().SetActiveWorksetId(previousWorkset);
            } catch(Exception) {
                log.Add("не удалось вернуть активный рабочий набор");
            }
        }
    }

    /// <summary>
    /// Делает активным рабочий набор связей, если он открыт. Возвращает прежний активный набор.
    /// </summary>
    private static WorksetId? SelectLinksWorkset(Document document, bool hasLinks, List<string> log) {
        if(!document.IsWorkshared || !hasLinks) {
            return null;
        }

        WorksetTable worksets = document.GetWorksetTable();
        WorksetId previousWorkset = worksets.GetActiveWorksetId();
        Workset linksWorkset = new FilteredWorksetCollector(document)
            .OfKind(WorksetKind.UserWorkset)
            .FirstOrDefault(workset => workset.Name == _linksWorkset);
        if(linksWorkset != null && linksWorkset.IsOpen) {
            worksets.SetActiveWorksetId(linksWorkset.Id);
            log.Add("связи размещаются в РН " + _linksWorkset);
        } else {
            log.Add("РН " + _linksWorkset + " " + (linksWorkset == null ? "отсутствует" : "закрыт")
                    + " — связи временно размещены в активном РН");
        }

        return previousWorkset;
    }

    private static void CreateLink(Document document, string linkPath, string fileName, List<string> log) {
        try {
            // все рабочие наборы связи открыты — иначе часть элементов не попадёт в спецификацию
            var options = new RevitLinkOptions(false, new WorksetConfiguration(WorksetConfigurationOption.OpenAllWorksets));
            LinkLoadResult loadResult = RevitLinkType.Create(
                document,
                ModelPathUtils.ConvertUserVisiblePathToModelPath(linkPath),
                options);
            if(loadResult.ElementId.IsNull() || document.GetElement(loadResult.ElementId) is not RevitLinkType) {
                log.Add("ОШИБКА подгрузки " + fileName + ": " + loadResult.LoadResult
                        + " (возможно, это копия той же модели или файл другой версии)");
                return;
            }

            try {
                RevitLinkInstance.Create(document, loadResult.ElementId, ImportPlacement.Shared);
                log.Add("временно подгружена: " + fileName + " (общие координаты)");
            } catch(Exception) {
                RevitLinkInstance.Create(document, loadResult.ElementId);
                log.Add("временно подгружена: " + fileName + " (ВНИМАНИЕ: начало к началу — общие координаты не настроены)");
            }
        } catch(Exception ex) {
            log.Add("ОШИБКА подгрузки " + fileName + ": " + ex.Message);
        }
    }

    private static ScheduleTableData ReadSchedule(Document document, string scheduleName, List<string> log) {
        ViewSchedule schedule = new FilteredElementCollector(document)
            .OfClass(typeof(ViewSchedule))
            .Cast<ViewSchedule>()
            .FirstOrDefault(view => !view.IsTemplate && view.Name == scheduleName);
        if(schedule == null) {
            return new ScheduleTableData { Name = scheduleName, IsMissing = true };
        }

        bool includedLinks = false;
        try {
            includedLinks = schedule.Definition.IncludeLinkedFiles;
            if(!includedLinks) {
                schedule.Definition.IncludeLinkedFiles = true;
            }
        } catch(Exception) {
            log.Add(scheduleName + ": не удалось включить элементы из связей");
        }

        document.Regenerate();
        return new ScheduleTableData {
            Name = scheduleName,
            IncludedLinksOriginally = includedLinks,
            HeaderRows = ReadSection(schedule, SectionType.Header, log),
            BodyRows = ReadSection(schedule, SectionType.Body, log)
        };
    }

    private static List<List<string>> ReadSection(ViewSchedule schedule, SectionType sectionType, List<string> log) {
        var rows = new List<List<string>>();
        try {
            TableSectionData section = schedule.GetTableData().GetSectionData(sectionType);
            for(int row = 0; row < section.NumberOfRows; row++) {
                var cells = new List<string>();
                for(int column = 0; column < section.NumberOfColumns; column++) {
                    cells.Add(CleanCellText(schedule.GetCellText(sectionType, row, column)));
                }

                rows.Add(cells);
            }
        } catch(Exception ex) {
            log.Add(schedule.Name + " " + sectionType + ": " + ex.Message);
        }

        return rows;
    }

    /// <summary>
    /// Управляющие символы (кроме переводов строк и табуляции) заменяются пробелом.
    /// </summary>
    private static string CleanCellText(string text) {
        return new string(text.Select(character =>
            character < 32 && character != '\n' && character != '\r' && character != '\t' ? ' ' : character).ToArray());
    }
}
