using System.IO;
using System.Text.RegularExpressions;

using RevitVORChecker.Core;
using RevitVORChecker.Models.Findings;

namespace RevitVORChecker.Models.Excel;

/// <summary>
/// Файлы с количеством вентблоков и УЭРВ: есть, заполнены по всем этажам, совпадают с этажностью ВОР.
/// </summary>
internal static class VentilationChecker {
    private const string _ventBlocks = "Вентблоки";
    private const string _airValves = "УЭРВ";

    public static void Check(
        string folder,
        List<string> fileNames,
        OrderedMap<string, List<VorRecord>> vorRecordsByBlock,
        FindingWriter writer) {

        var ventilationFiles = fileNames
            .Where(name => Regex.IsMatch(name, "вент[а]?блок|уэрв", RegexOptions.IgnoreCase)
                           && (name.ToLowerInvariant().EndsWith(".xlsx") || name.ToLowerInvariant().EndsWith(".xlsm"))
                           && !name.StartsWith("~$"))
            .ToList();
        if(ventilationFiles.Count == 0) {
            writer.Add(
                "В папке нет файлов с количеством вентблоков и УЭРВ (имя файла должно содержать "
                + "«Вентблок»/«Вентаблок»/«УЭРВ»). Данные о вентблоках и УЭРВ обязательны при сдаче ВОР "
                + "(статья «КР. ВОР. Основная информация», вкладка «Вентблоки»).");
            return;
        }

        var foundKinds = new HashSet<string>();
        foreach(string fileName in ventilationFiles) {
            ExcelComparison.TryProcessFile(fileName, writer, () => {
                using var workbook = new XlsxReader(Path.Combine(folder, fileName));
                foreach(var sheet in workbook.GetSheetPaths()) {
                    CheckSheet(fileName, sheet.Key, workbook.ReadSheet(sheet.Value), vorRecordsByBlock, foundKinds, writer);
                }
            });
        }

        foreach(string kind in new[] { _ventBlocks, _airValves }.Where(kind => !foundKinds.Contains(kind))) {
            writer.Add("Нет данных о количестве: " + kind + " (файлы: " + string.Join(", ", ventilationFiles) + ").");
        }
    }

    private static void CheckSheet(
        string fileName,
        string sheetName,
        List<SheetRow> rows,
        OrderedMap<string, List<VorRecord>> vorRecordsByBlock,
        HashSet<string> foundKinds,
        FindingWriter writer) {

        int headerIndex = -1;
        var columns = new Dictionary<string, int>();
        for(int rowIndex = 0; rowIndex < rows.Count; rowIndex++) {
            var columnByTitle = new Dictionary<string, int>();
            foreach(var cell in rows[rowIndex].Cells) {
                columnByTitle[ExcelText.Normalize(cell.Value.Text).ToLowerInvariant()] = cell.Key;
            }

            if(columnByTitle.ContainsKey("число") && (columnByTitle.ContainsKey("уровень") || columnByTitle.ContainsKey("этаж"))) {
                headerIndex = rowIndex;
                columns = columnByTitle;
                break;
            }
        }

        if(headerIndex < 0) {
            return;
        }

        int? FindColumn(string title) {
            return columns.TryGetValue(title, out int column) ? column : null;
        }

        int? blockColumn = FindColumn("блок");
        int? sectionColumn = FindColumn("секция");
        int? typeColumn = FindColumn("тип");
        int? countColumn = FindColumn("число");
        int? levelColumn = FindColumn("уровень") ?? FindColumn("этаж");

        string CellText(SheetRow row, int? column) {
            return column != null && row.Cells.TryGetValue(column.Value, out SheetCell cell) ? cell.Text : "";
        }

        var countsByGroup = new OrderedMap<string, VentilationGroup>();
        var nonNumericRows = new List<int>();
        foreach(SheetRow row in rows.Skip(headerIndex + 1)) {
            if(countColumn == null || !row.Cells.ContainsKey(countColumn.Value)) {
                continue;
            }

            double? count = ExcelText.ParseNumber(row.Cells[countColumn.Value].Text);
            if(count == null) {
                nonNumericRows.Add(row.RowNumber);
                continue;
            }

            string level = ExcelText.Normalize(CellText(row, levelColumn));
            string floor = GetFloor(level);
            string type = typeColumn != null ? ExcelText.Normalize(CellText(row, typeColumn)) : "";
            string kind = GetKind(type, sheetName, fileName);
            foundKinds.Add(kind);
            string block = ExcelText.Normalize(CellText(row, blockColumn));
            string section = ExcelText.Normalize(CellText(row, sectionColumn));
            VentilationGroup group = countsByGroup.GetOrAdd(
                kind + "\u0001" + block + "\u0001" + section,
                () => new VentilationGroup(kind, block, section));
            group.CountByFloor[floor] = (group.CountByFloor.TryGetValue(floor, out double current) ? current : 0) + count.Value;
        }

        if(nonNumericRows.Count > 0) {
            writer.Add(fileName + ": в столбце «Число» нечисловые значения (строки "
                       + string.Join(", ", nonNumericRows.Take(20)) + ").");
        }

        var sortedGroups = countsByGroup.Values
            .OrderBy(group => group.Kind, StringComparer.Ordinal)
            .ThenBy(group => group.Block, StringComparer.Ordinal)
            .ThenBy(group => group.Section, StringComparer.Ordinal);
        foreach(VentilationGroup group in sortedGroups) {
            CheckGroup(fileName, group, vorRecordsByBlock, writer);
        }
    }

    /// <summary>
    /// Этаж из значения столбца «Уровень»/«Этаж»: «03 этаж_К1_…» → «03», «3» → «03»; прочее — как есть.
    /// </summary>
    private static string GetFloor(string level) {
        Match floorMatch = Regex.Match(level, @"^(К|П|ТХ|Т|Д)?(\d{2}) этаж");
        if(floorMatch.Success) {
            return floorMatch.Groups[1].Value + floorMatch.Groups[2].Value;
        }

        // номер этажа числом: ключи этажей всегда двузначные, как в имени уровня
        return ExcelText.IsDigits(level) ? int.Parse(level).ToString("00") : level;
    }

    /// <summary>
    /// Вентблоки или УЭРВ: по типу строки, затем по имени листа, затем по имени файла —
    /// первое из них, где названо одно из двух.
    /// </summary>
    private static string GetKind(string type, string sheetName, string fileName) {
        foreach(string source in new[] { type, sheetName, fileName }) {
            string lowerSource = source.ToLowerInvariant();
            if(lowerSource.Contains("уэрв")) {
                return _airValves;
            }

            if(Regex.IsMatch(lowerSource, "вент[а]?блок")) {
                return _ventBlocks;
            }
        }

        return _ventBlocks;
    }

    private static void CheckGroup(
        string fileName,
        VentilationGroup group,
        OrderedMap<string, List<VorRecord>> vorRecordsByBlock,
        FindingWriter writer) {

        OrderedMap<string, double> countByFloor = group.CountByFloor;
        var floorNumbers = countByFloor.Keys.Where(ExcelText.IsDigits).Select(int.Parse).OrderBy(number => number).ToList();
        if(floorNumbers.Count == 0) {
            return;
        }

        int firstFloor = floorNumbers[0];
        int lastFloor = floorNumbers[floorNumbers.Count - 1];
        var missingFloors = Enumerable.Range(firstFloor, lastFloor - firstFloor + 1)
            .Select(number => number.ToString("00"))
            .Where(floor => !countByFloor.ContainsKey(floor))
            .ToList();
        string where = group.Kind + ", " + (group.Block.Length > 0 ? group.Block : "блок?") + ", "
                       + (group.Section.Length > 0 ? group.Section : "секция?");
        if(missingFloors.Count > 0) {
            writer.Add(fileName + ": " + where + " — нет данных на этажах " + string.Join(", ", missingFloors) + " (этажи "
                       + firstFloor.ToString("00") + "–" + lastFloor.ToString("00") + ").");
        }

        // типовое количество — самое частое на средних этажах (без первого и последнего)
        var middleFloors = floorNumbers.Count > 2 ? floorNumbers.Skip(1).Take(floorNumbers.Count - 2).ToList() : new List<int>();
        if(middleFloors.Count == 0) {
            middleFloors = floorNumbers;
        }

        var mostCommon = PythonFormat.MostCommon(middleFloors.Select(number =>
            countByFloor[number.ToString("00")].ToString("R", PythonFormat.Invariant)));
        double typicalCount = double.Parse(mostCommon[0].Key, PythonFormat.Invariant);
        var unusualFloors = middleFloors
            .Where(number => countByFloor[number.ToString("00")] != typicalCount)
            .Select(number => number.ToString("00") + " эт. — " + PythonFormat.GeneralFormat(countByFloor[number.ToString("00")]))
            .ToList();
        if(unusualFloors.Count > 0) {
            writer.Add(
                fileName + ": " + where + " — количество на этажах отличается от типового ("
                + PythonFormat.GeneralFormat(typicalCount) + " шт.): " + string.Join("; ", unusualFloors) + ". Проверить.",
                FindingKinds.Clarify);
        }

        CheckTopFloor(fileName, group, where, lastFloor, vorRecordsByBlock, writer);
    }

    /// <summary>
    /// Последний этаж с данными не ниже предпоследнего этажа корпуса в ВОР.
    /// </summary>
    private static void CheckTopFloor(
        string fileName,
        VentilationGroup group,
        string where,
        int lastFloor,
        OrderedMap<string, List<VorRecord>> vorRecordsByBlock,
        FindingWriter writer) {

        Match buildingNumber = Regex.Match(group.Block, @"(\d+)\s*$");
        var candidates = vorRecordsByBlock.Keys
            .Where(block => block.StartsWith("Корпус")
                            && buildingNumber.Success
                            && (block == group.Block
                                || block.EndsWith("." + buildingNumber.Groups[1].Value)
                                || block == "Корпус " + buildingNumber.Groups[1].Value))
            .ToList();
        if(candidates.Count != 1) {
            return;
        }

        var vorFloors = new HashSet<string>(vorRecordsByBlock[candidates[0]]
            .SelectMany(record => ExcelText.ParseFloors(record.Get("Этаж"))));
        var vorFloorNumbers = vorFloors.Where(ExcelText.IsDigits).Select(int.Parse).ToList();
        int topFloor = vorFloorNumbers.Count > 0 ? vorFloorNumbers.Max() : 0;
        if(topFloor != 0 && lastFloor < topFloor - 1) {
            writer.Add(
                fileName + ": " + where + " — последний этаж с данными " + lastFloor.ToString("00") + ", а в ВОР «"
                + candidates[0] + "» этажи до " + topFloor.ToString("00") + ".",
                FindingKinds.Clarify);
        }
    }

    /// <summary>
    /// Количество вентблоков или УЭРВ одной секции по этажам.
    /// </summary>
    private class VentilationGroup {
        public VentilationGroup(string kind, string block, string section) {
            Kind = kind;
            Block = block;
            Section = section;
        }

        public string Kind { get; }

        public string Block { get; }

        public string Section { get; }

        public OrderedMap<string, double> CountByFloor { get; } = new();
    }
}
