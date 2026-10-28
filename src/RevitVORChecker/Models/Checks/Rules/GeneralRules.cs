using System.Text.RegularExpressions;

using RevitVORChecker.Core;
using RevitVORChecker.Models.Data;
using RevitVORChecker.Models.Findings;

namespace RevitVORChecker.Models.Checks.Rules;

/// <summary>
/// Раздел 3 «Общие замечания»: спецификации ВОР, предупреждения, связи, уровни, файл, сведения о проекте, единицы.
/// </summary>
internal static class GeneralRules {
    private const long MaxFileSizeBytes = 500L * 1024 * 1024;

    /// <summary>
    /// Фрагменты текста предупреждений Revit, которые влияют на объёмы.
    /// </summary>
    private static readonly string[] _volumeWarnings = {
        "пересекаются", "перекрываются", "дублир", "одинаковые экземпляры", "одинаковых экземпляр",
        "в одном и том же месте", "Невозможно вырезать", "не удается вырезать", "не удалось соединить",
        "Невозможно соединить"
    };

    private static readonly Regex _levelNamePattern = new(@"^(К|П|Т|ТХ|Д)?\d{2} этаж_.+_-?\d+\.\d{3}$");

    private static readonly Regex _fileNamePattern =
        new(@"^[A-Z]{4}-[A-Z]{0,2}\d{2}(\.\d+)?_KR(_[A-Za-z0-9,\-\(\)\.]+)*$");

    private static readonly string[] _requiredProjectInfo = {
        "Статус проекта", "Заказчик", "Адрес проекта", "Наименование проекта", "Номер проекта",
        "ФОП_Абсолютная отметка нуля проекта"
    };

    public static void Check(ModelCheckContext context, FindingCollection findings) {
        if(!context.IsPitModel) {
            CheckVorSchedules(context, findings);
        }

        CheckWarnings(context, findings);
        CheckLinksAndImports(context, findings);
        CheckLevels(context, findings);
        if(!context.IsPitModel) {
            CheckNavisworksView(context, findings);
        }

        CheckFileName(context, findings);
        CheckProjectInfo(context, findings);
        CheckFile(context, findings);
        CheckUnits(context, findings);
    }

    private static void CheckVorSchedules(ModelCheckContext context, FindingCollection findings) {
        var schedules = new Dictionary<string, ScheduleData>();
        foreach(ScheduleData schedule in context.Model.Schedules) {
            schedules[schedule.Name] = schedule;
        }

        var requiredSchedules = new[] {
            ("ВОР_МТР_ТИП_КР", new[] {
                "Ключевая пометка BeginsWith г02", "Семейство и типоразмер NotContains Изоляция"
            }),
            ("ВОР_МТР_ТИП_КР_Изоляция", new[] {
                "Ключевая пометка BeginsWith г02", "Семейство и типоразмер Contains Изоляция"
            })
        };
        foreach((string scheduleName, string[] requiredFilters) in requiredSchedules) {
            if(!schedules.TryGetValue(scheduleName, out ScheduleData schedule)) {
                findings.Add(
                    ReportSections.General,
                    "В модели отсутствует спецификация «" + scheduleName + "» (скопировать из шаблона OneFile).",
                    null,
                    context.FileName);
                continue;
            }

            string filters = string.Join(" | ", schedule.Filters);
            foreach(string requiredFilter in requiredFilters.Where(filter => !filters.Contains(filter))) {
                findings.Add(
                    ReportSections.General,
                    "Фильтр спецификации «" + scheduleName + "» не соответствует статье «КР. ВОР. Основная информация»: "
                    + "нет условия «" + requiredFilter + "».",
                    null,
                    context.FileName,
                    detail: "сейчас: " + (filters.Length > 0 ? filters : "фильтров нет"));
            }

            int variantRows = schedule.Rows
                .Skip(1)
                .Count(row => row.Any(cell => (cell ?? "").ToLowerInvariant().Contains("варианты")));
            if(variantRows > 0) {
                findings.Add(
                    ReportSections.General,
                    "В спецификации «" + scheduleName + "» есть строки с «<варианты>» — элементы с разными значениями "
                    + "Блок/Секция/Этаж и др. слиты в одну строку ВОР.",
                    null,
                    context.FileName,
                    detail: "строк: " + variantRows);
            }
        }

        if(!schedules.ContainsKey("Параметры ВОР")) {
            findings.Add(
                ReportSections.General,
                "Нет проверочной спецификации «Параметры ВОР» (скопировать из шаблона OneFile).",
                null,
                context.FileName);
        }
    }

    private static void CheckWarnings(ModelCheckContext context, FindingCollection findings) {
        var elementIdsByWarning = new OrderedMap<string, List<long>>();
        foreach(WarningData warning in context.Model.Warnings) {
            string text = warning.Text ?? "";
            string lowerText = text.ToLowerInvariant();
            bool affectsVolume = _volumeWarnings.Any(fragment => lowerText.Contains(fragment.ToLowerInvariant()));
            if(!affectsVolume || lowerText.Contains("не пересекаются")) {
                continue;
            }

            List<long> elementIds = warning.ElementIds;
            if(elementIds.Count > 0 && elementIds.All(context.ExcludedElementIds.Contains)) {
                continue;
            }

            elementIdsByWarning.GetOrAdd(text.Trim(), () => new List<long>())
                .AddRange(elementIds.Where(id => !context.ExcludedElementIds.Contains(id)));
        }

        foreach(var warning in elementIdsByWarning) {
            List<long> elementIds = warning.Value;
            if(elementIds.Count > 0) {
                // пересечения уже есть в разделе 1
                elementIds = elementIds.Where(id => !context.CollisionElementIds.Contains(id)).ToList();
                if(elementIds.Count == 0) {
                    continue;
                }
            }

            string lowerText = warning.Key.ToLowerInvariant();
            bool isAboutLines = lowerText.Contains("линии") && !lowerText.Contains("эскиз");
            findings.Add(
                ReportSections.General,
                (isAboutLines ? "Проверить" : "Недопустимое, влияющее на объём,")
                + " предупреждение Revit (Стандарт п. 4.1.1): «" + warning.Key + "».",
                elementIds,
                context.FileName,
                isAboutLines ? FindingKinds.Clarify : FindingKinds.Remark);
        }
    }

    private static void CheckLinksAndImports(ModelCheckContext context, FindingCollection findings) {
        string fileName = context.FileName;
        var linksInWrongWorkset = new List<string>();
        foreach(LinkData link in context.Model.Links) {
            if(link.IsLoaded == false) {
                findings.Add(
                    ReportSections.General,
                    "В диспетчере связей есть незагруженные связи — удалить связи, не участвующие в оформлении "
                    + "(Стандарт п. 4.1.1).",
                    null,
                    fileName,
                    detail: link.Name);
            }

            foreach(string workset in link.InstanceWorksets) {
                bool isLinkWorkset = (workset ?? "").Replace("_", " ").Contains("Связи RVT")
                                     || (workset ?? "").Contains("Связи_RVT");
                if(!string.IsNullOrEmpty(workset) && !isLinkWorkset) {
                    linksInWrongWorkset.Add(link.Name + " → «" + workset + "»");
                }
            }
        }

        if(linksInWrongWorkset.Count > 0 && !context.IsPitModel) {
            findings.Add(
                ReportSections.General,
                "Связи RVT размещены в рабочих наборах без «Связи RVT» в имени — должны быть в РН "
                + "«Связи RVT_<раздел>_<блок>» (Стандарт п. 3.1.5).",
                null,
                fileName,
                detail: string.Join("; ", linksInWrongWorkset));
        }

        foreach(ImportData import in context.Model.Imports) {
            if(!import.IsLinked) {
                findings.Add(
                    ReportSections.General,
                    "В модели есть импортированный (не связанный) DWG — подложки DWG допускаются только как связь "
                    + "(Стандарт п. 3.1).",
                    new[] { import.Id },
                    fileName,
                    detail: import.Name);
            } else if(!import.IsViewSpecific && !(import.Workset ?? "").Contains("DWG")) {
                findings.Add(
                    ReportSections.General,
                    "Связанный DWG размещён не в РН «Связи DWG» (Стандарт п. 3.1.5).",
                    new[] { import.Id },
                    fileName,
                    detail: import.Name + " → «" + import.Workset + "»");
            }
        }

        if(context.Model.DirectShapeCount != 0) {
            findings.Add(
                ReportSections.General,
                "В модели есть элементы DirectShape (импортированная геометрия) — проверить, что конструкции "
                + "не выполнены импортом.",
                null,
                fileName,
                FindingKinds.Clarify,
                context.Model.DirectShapeCount + " шт.");
        }
    }

    private static void CheckLevels(ModelCheckContext context, FindingCollection findings) {
        List<LevelData> levels = context.Model.Levels;
        var badLevelNames = levels.Where(level => !_levelNamePattern.IsMatch(level.Name)).ToList();
        if(badLevelNames.Count > 0) {
            findings.Add(
                ReportSections.General,
                "Имена уровней не соответствуют Стандарту п. 2.9.3 («[Этаж]_[Блок]_[отметка через точку, 3 знака]»).",
                badLevelNames.Select(level => level.Id),
                context.FileName,
                detail: string.Join(", ", badLevelNames.Take(8).Select(level => level.Name)));
        }

        var levelsInWrongWorkset = levels
            .Where(level => !string.IsNullOrEmpty(level.Workset) && !level.Workset.Contains("Уровн"))
            .ToList();
        if(levelsInWrongWorkset.Count > 0) {
            findings.Add(
                ReportSections.General,
                "Уровни должны располагаться в отдельном рабочем наборе (Стандарт п. 3.1.2).",
                levelsInWrongWorkset.Select(level => level.Id),
                context.FileName);
        }
    }

    private static void CheckNavisworksView(ModelCheckContext context, FindingCollection findings) {
        View3DData navisworksView = context.Model.Views3D.FirstOrDefault(view => view.Name == "Navisworks");
        if(navisworksView == null) {
            findings.Add(
                ReportSections.General,
                "Нет 3D-вида «Navisworks» для выгрузки NWC (Стандарт п. 3.1.4).",
                null,
                context.FileName);
        } else if(navisworksView.IsCropBoxActive) {
            findings.Add(
                ReportSections.General,
                "На 3D-виде «Navisworks» включена граница 3D-вида — её необходимо отключить (Стандарт п. 3.1.4).",
                null,
                context.FileName);
        }
    }

    private static void CheckFileName(ModelCheckContext context, FindingCollection findings) {
        string fileName = context.FileName;
        if(_fileNamePattern.IsMatch(fileName)) {
            return;
        }

        var reasons = new List<string>();
        if(Regex.IsMatch(fileName, "[А-Яа-яЁё]")) {
            reasons.Add("кириллица");
        }

        if(!fileName.Contains("_KR")) {
            reasons.Add("нет шифра раздела KR");
        }

        findings.Add(
            ReportSections.General,
            "Имя файла не соответствует Стандарту ([Проект]-[Дом]_KR_[Корпус]_…, латиница). "
            + "Пример: PRKT-01_KR_PRK.rvt.",
            null,
            fileName,
            detail: fileName + " (" + (reasons.Count > 0 ? string.Join(", ", reasons) : "формат") + ")");
    }

    private static void CheckProjectInfo(ModelCheckContext context, FindingCollection findings) {
        Dictionary<string, string> projectInfo = context.Model.ProjectInfo ?? new Dictionary<string, string>();
        string GetInfo(string name) {
            return projectInfo.TryGetValue(name, out string value) ? value : null;
        }

        var emptyParameters = _requiredProjectInfo
            .Where(name => {
                string value = PythonFormat.Trim(GetInfo(name));
                var placeholders = new[] { name.ToLowerInvariant(), "дата", "-", "—" };
                return value.Length == 0 || placeholders.Contains(value.ToLowerInvariant());
            })
            .ToList();
        if(emptyParameters.Count > 0) {
            findings.Add(
                ReportSections.General,
                "В «Сведениях о проекте» не заполнены обязательные параметры (пустые или со значением-заглушкой "
                + "по умолчанию).",
                null,
                context.FileName,
                detail: string.Join(", ", emptyParameters.Select(name => "«" + name + "»")));
        }

        string status = PythonFormat.Trim(GetInfo("Статус проекта"));
        string stage = PythonFormat.Trim(context.Project.Stage ?? "П").ToUpperInvariant();
        string[] allowedStatuses = stage.StartsWith("П")
            ? new[] { "П", "ПД", "СТАДИЯ П", "ПРОЕКТНАЯ ДОКУМЕНТАЦИЯ" }
            : stage.StartsWith("Р")
                ? new[] { "Р", "РД", "СТАДИЯ Р", "СТАДИЯ РД", "РАБОЧАЯ ДОКУМЕНТАЦИЯ" }
                : new[] { stage };
        if(status.Length > 0
           && !emptyParameters.Contains("Статус проекта")
           && !allowedStatuses.Contains(status.ToUpperInvariant())) {
            findings.Add(
                ReportSections.General,
                "«Статус проекта» в «Сведениях о проекте» не соответствует стадии проверки (" + stage + ").",
                null,
                context.FileName,
                detail: "в модели «" + status + "»");
        }
    }

    private static void CheckFile(ModelCheckContext context, FindingCollection findings) {
        ModelFileData file = context.Model.File;
        long size = file.SizeBytes ?? -1;
        if(size != 0 && size > MaxFileSizeBytes) {
            findings.Add(
                ReportSections.General,
                "Размер файла превышает 500 МБ (Стандарт п. 2.8).",
                null,
                context.FileName,
                detail: PythonFormat.FormatFixed(size / 1048576.0, 0) + " МБ");
        }

        if(!(file.RevitVersion ?? "").Contains("2024")) {
            findings.Add(
                ReportSections.General,
                "Модель выполнена не в Revit 2024 (Стандарт табл. 2.5.1).",
                null,
                context.FileName);
        }
    }

    private static void CheckUnits(ModelCheckContext context, FindingCollection findings) {
        var requiredUnits = new[] {
            (Quantity: "length", Unit: "millimeters", Accuracy: 0.1, AccuracyText: "0,1", Title: "длина, мм"),
            (Quantity: "area", Unit: "squareMeters", Accuracy: 0.01, AccuracyText: "0,01", Title: "площадь, м²"),
            (Quantity: "volume", Unit: "cubicMeters", Accuracy: 0.01, AccuracyText: "0,01", Title: "объём, м³"),
            (Quantity: "angle", Unit: "degrees", Accuracy: 0.01, AccuracyText: "0,01", Title: "угол, °")
        };
        Dictionary<string, string> units = context.Model.Units ?? new Dictionary<string, string>();
        foreach(var required in requiredUnits) {
            string unitText = units.TryGetValue(required.Quantity, out string value) ? value ?? "" : "";
            Match match = Regex.Match(unitText, @"unit:(\w+)-.*acc=([\d.Ee-]+)");
            bool isCorrect = match.Success
                             && match.Groups[1].Value == required.Unit
                             && Math.Abs(double.Parse(match.Groups[2].Value, PythonFormat.Invariant) - required.Accuracy)
                             <= 1e-9;
            if(!isCorrect) {
                string current = match.Success ? match.Groups[2].Value.Replace(".", ",") : unitText;
                findings.Add(
                    ReportSections.General,
                    "Единицы проекта не соответствуют Стандарту п. 2.6: " + required.Title
                    + " — точность должна быть " + required.AccuracyText + ".",
                    null,
                    context.FileName,
                    detail: "сейчас точность " + current);
            }
        }
    }
}
