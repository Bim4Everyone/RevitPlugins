using System.Text.RegularExpressions;

using RevitVORChecker.Core;
using RevitVORChecker.Models.Data;
using RevitVORChecker.Models.Findings;

namespace RevitVORChecker.Models.Checks.Rules;

/// <summary>
/// Раздел 2 «Целостность объемов, примыкание конструкций, детализация»: рабочие наборы, состав модели.
/// </summary>
internal static class IntegrityRules {
    /// <summary>
    /// Единица штучного учёта в едином написании (<see cref="WorkClassifier.NormalizeUnit"/>).
    /// </summary>
    private const string _pieceUnit = "шт.";

    private const string _precastFlightWithoutCode =
        "Сборные ж/б лестничные марши: у материала нет кода работы из Классификатора видов работ. Сборные марши "
        + "считаются поштучно — назначить материал с кодом работы «Устройство сборных ж/б лестничных маршей» (шт.).";

    private const string _precastFlightNotPieceWork =
        "Сборные ж/б лестничные марши: код работы материала в Классификаторе измеряется не в штуках. Сборные марши "
        + "считаются поштучно — назначить материал с кодом работы «Устройство сборных ж/б лестничных маршей» (шт.).";

    /// <summary>
    /// Рабочие наборы, предназначенные для геометрии ВОР.
    /// </summary>
    private static readonly string[] _vorWorksets = {
        "02_КР_01_Конструкции_Фундаменты",
        "02_КР_02_Конструкции_Нулевой цикл",
        "02_КР_03_Конструкции_Надземный цикл",
        "02_КР_08_Конструкции_Металлические",
        "02_КР_09_Изоляция"
    };

    public static void Check(ModelCheckContext context, FindingCollection findings) {
        string fileName = context.FileName;

        // замечание — только РН «Скрытые»; «На скрытие» допустим (его элементы просто исключаются из проверки)
        var hiddenWorksets = context.Model.Worksets
            .Where(workset => workset.IndexOf("скрытые", StringComparison.OrdinalIgnoreCase) >= 0)
            .ToList();
        if(hiddenWorksets.Count > 0) {
            findings.Add(
                ReportSections.Integrity,
                "В модели есть рабочие наборы со словом «Скрытые» в названии — не допускаются "
                + "(элементы в них не попадают в ВОР).",
                null,
                fileName,
                detail: string.Join(", ", hiddenWorksets));
        }

        if(!context.IsPitModel) {
            CheckMissingKinds(context, findings, fileName);
            CheckVorWorksets(context, findings, fileName);
            CheckElementsWithoutVorMaterial(context, findings, fileName);
            CheckConstructionCycle(context, findings, fileName);
            CheckPrecastFlights(context, findings, fileName);
        }

        var paintedElements = context.Elements.Where(element => element.PaintMaterialIds.Count > 0).ToList();
        if(paintedElements.Count > 0) {
            findings.Add(
                ReportSections.Integrity,
                "Конструкции раскрашены инструментом «Краска» — запрещено, искажает материалы и объёмы ВОР.",
                paintedElements.Select(element => element.Id),
                fileName);
        }
    }

    /// <summary>
    /// Виды конструкций ВОР, которых нет в модели вообще.
    /// </summary>
    private static void CheckMissingKinds(ModelCheckContext context, FindingCollection findings, string fileName) {
        var kinds = new OrderedMap<string, string[]> {
            ["Бетонная подготовка"] = new[] { "БПодготовка" },
            ["Фундаментная плита"] = new[] { "ФПлита" },
            ["Перекрытия/покрытия"] = new[] { "Перекрытие", "Покрытие" },
            ["Стены"] = new[] { "Стена" },
            ["Пилоны/колонны"] = new[] { "Пилон", "Колонна" },
            ["Парапеты"] = new[] { "Парапет" },
            ["Изоляция"] = new[] { "Изол" },
            ["Термовкладыши"] = new[] { "Термовкладыш" },
            ["Засыпка"] = new[] { "Засыпка" },
            ["Лестничные марши/площадки"] = new[] { "ЛМарш", "ЛПлощадка" },
            ["Деформационные швы (изоляция шовная)"] = new[] { "Шов", "деф.шв", "Деф.шв" }
        };

        // вид считается отсутствующим, только если таких элементов нет в модели вообще
        // (элементы без кода «г02» — отдельное замечание);
        // признак вида ищется в имени типа и семейства (швы бывают семейством «КЖ_Заполнение деф.шва_…»)
        var modeledElements = context.Elements.Where(element => !ModelCheckContext.IsOpeningOrVoid(element)).ToList();
        var missingKinds = kinds
            .Where(kind => !modeledElements.Any(element => ModelCheckContext.ContainsAny(
                element.TypeName + " " + (element.FamilyName ?? ""),
                kind.Value)))
            .Select(kind => kind.Key)
            .ToList();
        if(missingKinds.Count > 0) {
            findings.Add(
                ReportSections.Integrity,
                "В модели отсутствуют элементы ВОР некоторых видов — проверить, все ли объёмы для смет замоделированы "
                + "(статья «КР. ВОР. Основная информация», вкладка «Объемы»).",
                null,
                fileName,
                FindingKinds.Clarify,
                string.Join(", ", missingKinds));
        }
    }

    private static void CheckVorWorksets(ModelCheckContext context, FindingCollection findings, string fileName) {
        // проверку распределения элементов ВОР по РН можно отключить в окне плагина
        if(!context.CheckWorksets) {
            return;
        }

        var elementsByWorkset = new OrderedMap<string, List<ElementData>>();
        foreach(ElementData element in context.VorElements) {
            string workset = element.Workset ?? "";
            if(workset.Length > 0 && !_vorWorksets.Any(workset.Contains)) {
                elementsByWorkset.GetOrAdd(workset, () => new List<ElementData>()).Add(element);
            }
        }

        foreach(var worksetElements in elementsByWorkset) {
            findings.Add(
                ReportSections.Integrity,
                "Элементы ВОР размещены в рабочем наборе, не предназначенном для геометрии ВОР "
                + "(допустимы 02_КР_01_…Фундаменты, 02_КР_02_…Нулевой цикл, 02_КР_03_…Надземный цикл, "
                + "02_КР_08_…Металлические, 02_КР_09_Изоляция).",
                worksetElements.Value.Select(element => element.Id),
                fileName,
                detail: "РН «" + worksetElements.Key + "»: "
                        + ModelCheckContext.DescribeTypes(worksetElements.Value, 4));
        }
    }

    private static void CheckElementsWithoutVorMaterial(
        ModelCheckContext context,
        FindingCollection findings,
        string fileName) {

        bool IsAuxiliaryMaterial(long materialId) {
            MaterialData material = context.Materials.TryGetValue(materialId, out MaterialData found)
                ? found
                : new MaterialData();
            return (material.Keynote ?? "").StartsWith("Вспомогат") || (material.Name ?? "").StartsWith("Вспомогат");
        }

        var elementsOutsideVor = context.Elements
            .Where(element => element.GetMaterialIds().Any()
                              && context.GetVorMaterials(element).Count == 0
                              && !ModelCheckContext.IsInExcludedWorkset(element)
                              && !ModelCheckContext.IsOpeningOrVoid(element)
                              && element.SuperComponentId == null
                              && !element.GetMaterialIds().All(IsAuxiliaryMaterial))
            .ToList();
        if(elementsOutsideVor.Count > 0) {
            var categoriesAndTypes = PythonFormat.SortOrdinal(elementsOutsideVor
                .Select(element => element.Category + ": " + element.TypeName)
                .Distinct());
            findings.Add(
                ReportSections.Integrity,
                "Элементы не попадут в спецификации ВОР: у их материалов нет кода «г02…» в ключевой пометке. "
                + "Назначить материал по Стандарту А101 или перенести в РН вспомогательных конструкций.",
                elementsOutsideVor.Select(element => element.Id),
                fileName,
                detail: string.Join(", ", categoriesAndTypes.Take(6)));
        }
    }

    /// <summary>
    /// Сборные ж/б лестничные марши считаются поштучно: материал с кодом работы Классификатора, единица работы
    /// которой «шт.» («Устройство сборных ж/б лестничных маршей»). Материал «Бетон» с кодом «г02» — только для
    /// монолитных конструкций (статья «КР. ВОР. Основная информация»).
    /// </summary>
    /// <remarks>
    /// Параметры такого материала (глава, работа, ФОП_МТР_Единица измерения «шт», тип подсчёта 4) проверяются
    /// по Классификатору в разделах материалов (<see cref="MaterialRules"/>), здесь не повторяются.
    /// </remarks>
    private static void CheckPrecastFlights(ModelCheckContext context, FindingCollection findings, string fileName) {
        var problems = new GroupedElements();
        IEnumerable<ElementData> precastFlights = context.Elements
            .Where(element => ModelCheckContext.IsPrecastFlight(element) && !ModelCheckContext.IsOpeningOrVoid(element));
        foreach(ElementData flight in precastFlights) {
            List<MaterialData> materials = flight.GetMaterialIds()
                .Where(context.Materials.ContainsKey)
                .Select(id => context.Materials[id])
                .ToList();
            List<MaterialData> classifiedMaterials = materials
                .Where(material => context.Classifier.Works.ContainsKey(PythonFormat.Trim(material.Keynote)))
                .ToList();
            if(classifiedMaterials.Count == 0) {
                problems.Add(_precastFlightWithoutCode, DescribeMaterials(materials), flight);
                continue;
            }

            bool hasPieceWork = classifiedMaterials
                .Any(material => context.Classifier.Works[PythonFormat.Trim(material.Keynote)].Unit == _pieceUnit);
            if(!hasPieceWork) {
                problems.Add(_precastFlightNotPieceWork, DescribeMaterials(classifiedMaterials), flight);
            }
        }

        problems.AddFindings(findings, ReportSections.Integrity, fileName, maxTypeCount: 4);
    }

    /// <summary>
    /// Материалы для уточнения замечания: имя и код ключевой пометки.
    /// </summary>
    private static string DescribeMaterials(List<MaterialData> materials) {
        if(materials.Count == 0) {
            return "материал не назначен";
        }

        IEnumerable<string> descriptions = materials
            .Select(material => "«" + material.Name + "» (код «"
                                + (PythonFormat.Trim(material.Keynote) is { Length: > 0 } keynote ? keynote : "не задан")
                                + "»)");
        return "материалы: " + string.Join(", ", PythonFormat.SortOrdinal(descriptions));
    }

    private static void CheckConstructionCycle(ModelCheckContext context, FindingCollection findings, string fileName) {
        var elementsWithoutCycle = context.VorElements
            .Where(element => !Regex.IsMatch(element.TypeName, "^(Ф|НН|ВН)_"))
            .ToList();
        if(elementsWithoutCycle.Count > 0) {
            findings.Add(
                ReportSections.Integrity,
                "Имя типоразмера не начинается с цикла возведения «Ф_», «НН_», «ВН_» — объёмы нельзя распределить по циклам.",
                elementsWithoutCycle.Select(element => element.Id),
                fileName,
                detail: ModelCheckContext.DescribeTypes(elementsWithoutCycle));
        }
    }
}
