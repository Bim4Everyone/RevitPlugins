using RevitVORChecker.Core;
using RevitVORChecker.Models.Data;
using RevitVORChecker.Models.Findings;

namespace RevitVORChecker.Models.Excel;

/// <summary>
/// Сверка «КЖ_Изоляция_(+Прямой расчет)»: суммы количества по позициям (наименование, единица, норма расхода).
/// </summary>
internal static class DirectCalculationComparer {
    public static void Compare(ScheduleTableData schedule, List<SheetRow> excelRows, string fileName, FindingWriter writer) {
        OrderedMap<string, CalculationItem> modelItems = ReadModelItems(schedule);
        (OrderedMap<string, CalculationItem> excelItems, List<(int RowNumber, string Value)> textQuantities) =
            ReadExcelItems(excelRows);

        var differences = new List<List<string>>();
        foreach(var modelItem in modelItems) {
            CalculationItem item = modelItem.Value;
            (double? modelSum, int decimals) = Sum(item.Quantities);
            string rate = item.Rate.Length > 0 ? item.Rate : "—";
            if(!excelItems.TryGetValue(modelItem.Key, out CalculationItem excelItem)) {
                differences.Add(new List<string> {
                    item.Name, item.Unit, rate, FormatSum(modelSum, decimals), "нет", "позиции нет в Excel"
                });
                continue;
            }

            (double? excelSum, _) = Sum(excelItem.Quantities);
            if(modelSum == null && excelSum == null) {
                continue;
            }

            if(modelSum == null
               || excelSum == null
               || PythonFormat.Round(excelSum.Value, decimals) != PythonFormat.Round(modelSum.Value, decimals)) {
                differences.Add(new List<string> {
                    item.Name, item.Unit, rate, FormatSum(modelSum, decimals), FormatSum(excelSum, decimals),
                    "другое количество"
                });
            }
        }

        foreach(var excelItem in excelItems) {
            if(modelItems.ContainsKey(excelItem.Key)) {
                continue;
            }

            CalculationItem item = excelItem.Value;
            (double? excelSum, int decimals) = Sum(item.Quantities);
            differences.Add(new List<string> {
                item.Name, item.Unit, item.Rate.Length > 0 ? item.Rate : "—", "нет", FormatSum(excelSum, decimals),
                "позиции нет в модели"
            });
        }

        string scheduleName = schedule.Name;
        if(differences.Count > 0) {
            writer.Add(
                "Файл «" + fileName + "» не соответствует спецификации «" + scheduleName + "» сводной модели: "
                + "не совпадают " + differences.Count + " позиций (количество — сумма по позиции).",
                FindingKinds.Remark,
                new FindingTable {
                    Header = new List<string> { "Наименование", "Ед. изм.", "Н.расх.", "Модель", "Excel", "Примечание" },
                    Rows = differences
                });
        }

        if(textQuantities.Count > 0) {
            writer.Add(
                "Файл «" + fileName + "»: количество записано текстом в строках "
                + string.Join(", ", textQuantities.Select(item => item.RowNumber.ToString()))
                + " (" + string.Join("; ", textQuantities.Select(item => item.Value)) + ").");
        }

        if(differences.Count == 0) {
            writer.Add(
                "Файл «" + fileName + "» ↔ спецификация «" + scheduleName + "»: данные идентичны ("
                + modelItems.Count + " позиций).",
                FindingKinds.Info);
        }
    }

    private static OrderedMap<string, CalculationItem> ReadModelItems(ScheduleTableData schedule) {
        var items = new OrderedMap<string, CalculationItem>();
        foreach(List<string> row in schedule.HeaderRows.Concat(schedule.BodyRows)) {
            var cells = row.Select(ExcelText.Normalize).ToList();
            int unitIndex = cells.FindIndex(ExcelText.Units.Contains);
            if(unitIndex < 0) {
                continue;
            }

            string name = cells.Take(unitIndex).FirstOrDefault(cell => cell.Length > 0 && ExcelText.ParseNumber(cell) == null)
                          ?? "";
            if(name.Length == 0 || name == "Наименование") {
                continue;
            }

            var numbers = cells.Skip(unitIndex + 1).Where(cell => ExcelText.ParseNumber(cell) != null).ToList();
            string quantity = numbers.Count > 0 ? numbers[0] : "";
            string rate = numbers.Count > 1 ? PythonFormat.GeneralFormat(ExcelText.ParseNumber(numbers[1]).Value) : "";
            var item = new CalculationItem(name, cells[unitIndex], rate);
            items.GetOrAdd(item.Code, () => item).Quantities.Add(quantity);
        }

        return items;
    }

    private static (OrderedMap<string, CalculationItem> Items, List<(int RowNumber, string Value)> TextQuantities)
        ReadExcelItems(List<SheetRow> rows) {

        var items = new OrderedMap<string, CalculationItem>();
        var textQuantities = new List<(int RowNumber, string Value)>();
        foreach(SheetRow row in rows) {
            var cells = new Dictionary<int, (string Text, bool IsText)>();
            foreach(var cell in row.Cells) {
                cells[cell.Key] = (ExcelText.Normalize(cell.Value.Text), cell.Value.IsText);
            }

            var columnOrder = cells.Keys.OrderBy(column => column).ToList();
            int? unitColumn = columnOrder.Cast<int?>().FirstOrDefault(column => ExcelText.Units.Contains(cells[column.Value].Text));
            if(unitColumn == null) {
                continue;
            }

            string name = columnOrder
                .Where(column => column < unitColumn
                                 && cells[column].Text.Length > 0
                                 && ExcelText.ParseNumber(cells[column].Text) == null)
                .Select(column => cells[column].Text)
                .FirstOrDefault() ?? "";
            if(name.Length == 0 || name == "Наименование") {
                continue;
            }

            var numberColumns = columnOrder
                .Where(column => column > unitColumn && ExcelText.ParseNumber(cells[column].Text) != null)
                .ToList();
            int? quantityColumn = numberColumns.Count > 0 ? numberColumns[0] : null;
            string quantity = quantityColumn != null ? cells[quantityColumn.Value].Text : "";
            string rate = numberColumns.Count > 1
                ? PythonFormat.GeneralFormat(ExcelText.ParseNumber(cells[numberColumns[1]].Text).Value)
                : "";
            if(quantityColumn != null && cells[quantityColumn.Value].IsText) {
                textQuantities.Add((row.RowNumber, quantity));
            }

            var item = new CalculationItem(name, cells[unitColumn.Value].Text, rate);
            items.GetOrAdd(item.Code, () => item).Quantities.Add(quantity);
        }

        return (items, textQuantities);
    }

    /// <summary>
    /// Сумма числовых количеств и наибольшее число знаков после запятой (по умолчанию 2).
    /// </summary>
    private static (double? Sum, int Decimals) Sum(List<string> quantities) {
        var numericQuantities = quantities.Where(quantity => ExcelText.ParseNumber(quantity) != null).ToList();
        if(numericQuantities.Count == 0) {
            return (null, 2);
        }

        double sum = 0;
        foreach(string quantity in numericQuantities) {
            sum += ExcelText.ParseNumber(quantity).Value;
        }

        return (sum, numericQuantities.Select(ExcelText.CountDecimals).Max());
    }

    private static string FormatSum(double? sum, int decimals) {
        return sum == null ? "нет" : PythonFormat.FormatFixed(sum.Value, decimals).Replace(".", ",");
    }

    /// <summary>
    /// Позиция прямого расчёта: наименование, единица, норма расхода и количества по строкам.
    /// </summary>
    private class CalculationItem {
        public CalculationItem(string name, string unit, string rate) {
            Name = name;
            Unit = unit;
            Rate = rate;
        }

        public string Name { get; }

        public string Unit { get; }

        public string Rate { get; }

        public string Code => Name + "\u0001" + Unit + "\u0001" + Rate;

        public List<string> Quantities { get; } = new();
    }
}
