using RevitVORChecker.Core;

namespace RevitVORChecker.Models.Excel;

/// <summary>
/// Строка листа «Исходная ВОР»: заголовок столбца → значение.
/// </summary>
internal class VorRecord {
    public VorRecord(int rowNumber, OrderedMap<string, string> values) {
        RowNumber = rowNumber;
        Values = values;
    }

    public int RowNumber { get; }

    public OrderedMap<string, string> Values { get; }

    public string Get(string column) {
        return Values.TryGetValue(column, out string value) ? value : "";
    }

    /// <summary>
    /// Строки данных под строкой заголовков (со столбцами «Количество», «Блок», «Тип»).
    /// </summary>
    public static List<VorRecord> ReadAll(List<SheetRow> rows) {
        for(int rowIndex = 0; rowIndex < rows.Count; rowIndex++) {
            var columnByTitle = new OrderedMap<string, int>();
            foreach(var cell in rows[rowIndex].Cells) {
                columnByTitle[ExcelText.Normalize(cell.Value.Text)] = cell.Key;
            }

            if(!columnByTitle.ContainsKey("Количество")
               || !columnByTitle.ContainsKey("Блок")
               || !columnByTitle.ContainsKey("Тип")) {
                continue;
            }

            var columns = columnByTitle.Where(column => column.Key.Length > 0).ToList();
            var records = new List<VorRecord>();
            foreach(SheetRow row in rows.Skip(rowIndex + 1)) {
                var values = new OrderedMap<string, string>();
                foreach(var column in columns) {
                    values[column.Key] = row.Cells.TryGetValue(column.Value, out SheetCell cell)
                        ? ExcelText.Normalize(cell.Text)
                        : "";
                }

                if(values.Values.Any(value => value.Length > 0)) {
                    records.Add(new VorRecord(row.RowNumber, values));
                }
            }

            return records;
        }

        return new List<VorRecord>();
    }
}
