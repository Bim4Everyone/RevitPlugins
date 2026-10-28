using System.Text.RegularExpressions;

using RevitVORChecker.Core;
using RevitVORChecker.Models.Findings;
using RevitVORChecker.Models.Project;

namespace RevitVORChecker.Models.Excel;

/// <summary>
/// Целостность объёмов ВОР по корпусам и автостоянке: фундаментная плита, подземные этажи, этажи без конструкций.
/// </summary>
internal static class VolumeIntegrityChecker {
    private static readonly string[] _underParkingKeys = { "ФПлита", "БПодготовка", "ПСлой", "Засыпка", "риям", "утолщ", "Утолщ" };
    private static readonly string[] _verticalKeys = { "Стена", "Пилон", "Колонна" };

    /// <summary>
    /// Проверяет записи ВОР и возвращает их по блокам (для сверки вентблоков).
    /// </summary>
    public static OrderedMap<string, List<VorRecord>> Check(
        List<VorRecord> records,
        List<ModelSummary> models,
        ProjectCard project,
        string fileName,
        FindingWriter writer) {

        CheckVariants(records, fileName, writer);
        records = records.Where(record => !record.Get("Тип").Contains("Изол")).ToList();
        var recordsByBlock = new OrderedMap<string, List<VorRecord>>();
        foreach(VorRecord record in records) {
            var blocks = record.Get("Блок").Split(',').Select(block => block.Trim()).Where(block => block.Length > 0);
            foreach(string block in blocks) {
                recordsByBlock.GetOrAdd(block, () => new List<VorRecord>()).Add(record);
            }
        }

        var undergroundByBuilding = new Dictionary<string, int>();
        foreach(ModelSummary model in models.Where(model => model.Block.StartsWith("Корпус"))) {
            int known = undergroundByBuilding.TryGetValue(model.Block, out int count) ? count : 0;
            undergroundByBuilding[model.Block] = Math.Max(known, model.UndergroundFloorCount);
        }

        var buildings = PythonFormat.SortOrdinal(undergroundByBuilding.Keys
            .Concat(recordsByBlock.Keys.Where(block => block.StartsWith("Корпус")))
            .Distinct());
        string foundationRule = project.FoundationSlabRule ?? ProjectCard.KnowledgeBaseFoundationRule;
        foreach(string building in buildings) {
            List<VorRecord> buildingRecords = recordsByBlock.TryGetValue(building, out var found) ? found : new List<VorRecord>();
            int? undergroundFloors = undergroundByBuilding.TryGetValue(building, out int floors) ? floors : null;
            CheckBuilding(building, buildingRecords, undergroundFloors, foundationRule, fileName, writer);
        }

        bool hasParking = models.Any(model => model.Block == "Автостоянка") || recordsByBlock.ContainsKey("Автостоянка");
        if(hasParking) {
            List<VorRecord> parkingRecords = recordsByBlock.TryGetValue("Автостоянка", out var found)
                ? found
                : new List<VorRecord>();
            if(parkingRecords.Count == 0) {
                writer.Add(fileName + ": в ВОР нет объёмов «Автостоянка».");
            } else if(!HasType(parkingRecords, "ФПлита")) {
                writer.Add(fileName + ": в объёме «Автостоянка» нет фундаментной плиты.");
            }
        }

        return recordsByBlock;
    }

    private static void CheckVariants(List<VorRecord> records, string fileName, FindingWriter writer) {
        string[] columns = { "Блок", "Секция", "Этаж" };
        bool HasVariants(VorRecord record, string column) {
            return record.Get(column).ToLowerInvariant().Contains("варианты");
        }

        var variantRecords = records.Where(record => columns.Any(column => HasVariants(record, column))).ToList();
        if(variantRecords.Count == 0) {
            return;
        }

        var variantColumns = PythonFormat.SortOrdinal(variantRecords
            .SelectMany(record => columns.Where(column => HasVariants(record, column)))
            .Distinct());
        string rowNumbers = string.Join(", ", variantRecords.Take(40).Select(record => record.RowNumber.ToString()));
        writer.Add(
            fileName + ": " + variantRecords.Count + " строк ВОР содержат «<варианты>» в столбцах "
            + string.Join(", ", variantColumns) + " — объёмы разных блоков/секций/этажей слиты в одну строку, "
            + "ВОР не детализирован. Причина — разные значения ФОП_Блок/Секция/Этаж СМР у элементов одной строки "
            + "спецификации. Строки Excel: " + rowNumbers + (variantRecords.Count > 40 ? " …" : "") + ".");
    }

    private static void CheckBuilding(
        string building,
        List<VorRecord> records,
        int? undergroundFloors,
        string foundationRule,
        string fileName,
        FindingWriter writer) {

        if(records.Count == 0) {
            writer.Add(fileName + ": в ВОР нет ни одной строки блока «" + building + "» (модель корпуса есть в папке).");
            return;
        }

        var floors = new HashSet<string>(records.SelectMany(record => ExcelText.ParseFloors(record.Get("Этаж"))));
        bool hasFoundationSlab = HasType(records, "ФПлита");
        if(foundationRule == ProjectCard.ParkingFoundationRule && undergroundFloors >= 2) {
            if(hasFoundationSlab) {
                writer.Add(
                    fileName + ": под «" + building + "» " + undergroundFloors + " подземных этажа — фундаментная плита "
                    + "должна относиться к «Автостоянке», а в ВОР она в объёме корпуса.");
            }
        } else if(!hasFoundationSlab) {
            string undergroundNote = undergroundFloors != null ? "; подземных этажей: " + undergroundFloors : "";
            writer.Add(
                fileName + ": в объёме «" + building + "» нет фундаментной плиты (ФП в проекции корпуса относится "
                + "к корпусу — статья «КР. ВОР. Основная информация», таблица «Распределение объемов подземной части»"
                + undergroundNote + ").");
        }

        if(hasFoundationSlab && !HasType(records, "БПодготовка")) {
            writer.Add(
                fileName + ": в объёме «" + building + "» есть фундаментная плита, но нет бетонной подготовки.",
                FindingKinds.Clarify);
        }

        if(undergroundFloors >= 2) {
            var deepRecords = records
                .Where(record => ExcelText.ParseFloors(record.Get("Этаж")).Any(floor => Regex.IsMatch(floor, "^П0[2-9]$"))
                                 && !_underParkingKeys.Any(key => record.Get("Тип").Contains(key)))
                .ToList();
            if(deepRecords.Count > 0) {
                var types = PythonFormat.SortOrdinal(deepRecords.Select(record => record.Get("Тип")).Distinct()).Take(6);
                writer.Add(
                    fileName + ": в объёме «" + building + "» есть строки −2 этажа и ниже (" + string.Join(", ", types)
                    + ") — вертикальные конструкции −2 и ниже и плиты между подземными этажами относятся к «Автостоянке».");
            }
        }

        if(undergroundFloors >= 1 && !floors.Contains("П01")) {
            writer.Add(
                fileName + ": у «" + building + "» в модели " + undergroundFloors + " подземн. этаж(а), но в ВОР корпуса "
                + "нет объёмов «П01 этаж» (вертикали −1 этажа и плита над ним относятся к корпусу).");
        }

        CheckFloorsWithoutConstructions(building, records, floors, fileName, writer);
    }

    private static void CheckFloorsWithoutConstructions(
        string building,
        List<VorRecord> records,
        HashSet<string> floors,
        string fileName,
        FindingWriter writer) {

        var floorNumbers = floors.Where(ExcelText.IsDigits).Select(int.Parse).OrderBy(number => number).ToList();
        if(records.Any(record => record.Get("Этаж").ToLowerInvariant().Contains("варианты"))) {
            floorNumbers.Clear();
        }

        if(floorNumbers.Count == 0) {
            return;
        }

        var floorsWithoutSlabs = new List<string>();
        var floorsWithoutVerticals = new List<string>();
        for(int number = 1; number <= floorNumbers.Max(); number++) {
            string floor = number.ToString("00");
            var floorRecords = records.Where(record => ExcelText.ParseFloors(record.Get("Этаж")).Contains(floor)).ToList();
            if(!floorRecords.Any(record => record.Get("Тип").Contains("Перекрытие") || record.Get("Тип").Contains("Покрытие"))) {
                floorsWithoutSlabs.Add(floor);
            }

            if(!floorRecords.Any(record => _verticalKeys.Any(key => record.Get("Тип").Contains(key)))) {
                floorsWithoutVerticals.Add(floor);
            }
        }

        if(floorsWithoutSlabs.Count > 0) {
            writer.Add(
                fileName + ": «" + building + "» — нет перекрытий/покрытий на этажах " + string.Join(", ", floorsWithoutSlabs)
                + " (проверить целостность объёмов).",
                FindingKinds.Clarify);
        }

        if(floorsWithoutVerticals.Count > 0) {
            writer.Add(
                fileName + ": «" + building + "» — нет вертикальных конструкций на этажах "
                + string.Join(", ", floorsWithoutVerticals) + " (проверить целостность объёмов).",
                FindingKinds.Clarify);
        }
    }

    private static bool HasType(List<VorRecord> records, string key) {
        return records.Any(record => record.Get("Тип").Contains(key));
    }
}
