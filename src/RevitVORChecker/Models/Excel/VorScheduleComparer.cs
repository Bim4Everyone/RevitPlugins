using RevitVORChecker.Core;
using RevitVORChecker.Models.Data;
using RevitVORChecker.Models.Findings;

namespace RevitVORChecker.Models.Excel;

/// <summary>
/// Сверка листа ВОР в Excel со спецификацией сводной модели построчно (с учётом кратности строк).
/// </summary>
internal class VorScheduleComparer {
    private const string _quantityColumn = "Количество";

    /// <summary>
    /// Столбцы, которые показываются в таблице расхождений.
    /// </summary>
    private static readonly string[] _displayColumns = { "Блок", "Секция", "Этаж", "Тип", "Материал", "Назначение", "Ед. изм." };

    private readonly Dictionary<string, int> _precisionByColumn = new();
    private readonly List<(int RowNumber, string Value)> _textQuantities = new();
    private List<string> _header = new();

    /// <summary>
    /// Сверяет таблицу Excel <paramref name="excelRows"/> со спецификацией <paramref name="schedule"/>.
    /// </summary>
    /// <param name="insulationSchedule">
    /// Спецификация изоляции: её строки на листе «Исходная ВОР» выносятся в отдельное замечание.
    /// </param>
    public void Compare(
        ScheduleTableData schedule,
        List<SheetRow> excelRows,
        string fileName,
        FindingWriter writer,
        ScheduleTableData? insulationSchedule = null) {

        string scheduleName = schedule.Name;
        (List<string>? header, List<List<string>> modelRows) = ReadModelTable(schedule);
        if(header == null) {
            writer.Add(
                "Спецификация «" + scheduleName + "» в модели не прочитана (нет столбца «Количество»).",
                FindingKinds.Clarify);
            return;
        }

        _header = header;
        (OrderedMap<string, int?>? columns, List<(int RowNumber, Dictionary<string, SheetCell?> Cells)> excelData) =
            ReadExcelTable(excelRows, header);
        if(columns == null) {
            writer.Add(fileName + ": не найдена строка заголовков со столбцами спецификации «" + scheduleName + "» ("
                       + string.Join(", ", header) + ").");
            return;
        }

        var missingColumns = columns.Where(column => column.Value == null).Select(column => column.Key).ToList();
        if(missingColumns.Count > 0) {
            writer.Add(fileName + ": в Excel нет столбцов спецификации: " + string.Join(", ", missingColumns) + ".");
        }

        foreach(List<string> row in modelRows) {
            for(int index = 0; index < header.Count; index++) {
                if(index < row.Count && ExcelText.ParseNumber(row[index]) != null) {
                    _precisionByColumn[header[index]] = Math.Max(GetPrecision(header[index]), ExcelText.CountDecimals(row[index]));
                }
            }
        }

        var modelBag = new RowKeyBag();
        foreach(List<string> row in modelRows) {
            modelBag.Add(CreateModelKey(row));
        }

        var excelBag = new RowKeyBag();
        foreach(var row in excelData) {
            excelBag.Add(CreateExcelKey(row.RowNumber, row.Cells));
        }

        RowKeyBag missingInExcel = RowKeyBag.Subtract(modelBag, excelBag);
        RowKeyBag extraInExcel = RowKeyBag.Subtract(excelBag, modelBag);
        if(insulationSchedule != null && !extraInExcel.IsEmpty) {
            extraInExcel = SeparateInsulationRows(insulationSchedule, extraInExcel, fileName, writer);
        }

        List<List<string>> differences = DescribeDifferences(missingInExcel.ToList(), extraInExcel.ToList());
        if(differences.Count > 0) {
            var displayColumns = _displayColumns.Where(header.Contains).ToList();
            writer.Add(
                "Файл «" + fileName + "» не соответствует спецификации «" + scheduleName + "» сводной модели: "
                + "не совпадают " + differences.Count + " строк (строк в модели " + modelRows.Count + ", в Excel "
                + excelData.Count + "). Ниже — все несовпадающие строки (модель / Excel); «отличаются» — совпадают "
                + "видимые столбцы, но различаются прочие (модель / Excel).",
                FindingKinds.Remark,
                new FindingTable {
                    Header = displayColumns.Concat(new[] { "Модель", "Excel", "Примечание" }).ToList(),
                    Rows = differences
                });
        }

        if(_textQuantities.Count > 0) {
            string rowNumbers = string.Join(", ", _textQuantities.Take(30).Select(item => item.RowNumber.ToString()));
            writer.Add(
                "Файл «" + fileName + "»: в столбце «Количество» " + _textQuantities.Count
                + " значений записаны текстом (не числом) — они не попадут в суммы сводной таблицы. Строки Excel: "
                + rowNumbers + (_textQuantities.Count > 30 ? " …" : "") + ".");
        }

        if(differences.Count == 0) {
            writer.Add(
                "Файл «" + fileName + "» ↔ спецификация «" + scheduleName + "»: данные идентичны ("
                + modelRows.Count + " строк).",
                FindingKinds.Info);
        }
    }

    /// <summary>
    /// Шапка и строки данных спецификации модели (строки с заполненным количеством).
    /// </summary>
    public static (List<string>? Header, List<List<string>> Rows) ReadModelTable(ScheduleTableData schedule) {
        List<List<string>> body = schedule.BodyRows;
        int headerIndex = body.FindIndex(row => row.Contains(_quantityColumn));
        if(headerIndex < 0) {
            return (null, new List<List<string>>());
        }

        var header = body[headerIndex].Select(ExcelText.Normalize).ToList();
        int quantityIndex = header.IndexOf(_quantityColumn);
        var rows = body.Skip(headerIndex + 1)
            .Where(row => row.Any(cell => PythonFormat.Trim(cell).Length > 0)
                          && PythonFormat.Trim(row[quantityIndex]).Length > 0)
            .ToList();
        return (header, rows);
    }

    /// <summary>
    /// Находит в листе Excel строку заголовков со столбцами спецификации и читает строки данных под ней.
    /// </summary>
    private static (OrderedMap<string, int?>? Columns, List<(int RowNumber, Dictionary<string, SheetCell?> Cells)> Data)
        ReadExcelTable(List<SheetRow> rows, List<string> requiredColumns) {

        for(int rowIndex = 0; rowIndex < rows.Count; rowIndex++) {
            var columnByTitle = new Dictionary<string, int>();
            foreach(var cell in rows[rowIndex].Cells) {
                columnByTitle[ExcelText.Normalize(cell.Value.Text)] = cell.Key;
            }

            int foundColumns = requiredColumns.Count(columnByTitle.ContainsKey);
            if(!columnByTitle.ContainsKey(_quantityColumn) || foundColumns < Math.Max(3, requiredColumns.Count - 2)) {
                continue;
            }

            var columns = new OrderedMap<string, int?>();
            foreach(string column in requiredColumns) {
                columns[column] = columnByTitle.TryGetValue(column, out int index) ? index : null;
            }

            var data = new List<(int RowNumber, Dictionary<string, SheetCell?> Cells)>();
            foreach(SheetRow row in rows.Skip(rowIndex + 1)) {
                if(!row.Cells.Values.Any(cell => PythonFormat.Trim(cell.Text).Length > 0)) {
                    continue;
                }

                var cells = new Dictionary<string, SheetCell?>();
                foreach(var column in columns) {
                    if(column.Value == null) {
                        continue;
                    }

                    cells[column.Key] = row.Cells.TryGetValue(column.Value.Value, out SheetCell cell) ? cell : null;
                }

                data.Add((row.RowNumber, cells));
            }

            return (columns, data);
        }

        return (null, new List<(int RowNumber, Dictionary<string, SheetCell?> Cells)>());
    }

    private int GetPrecision(string column) {
        return _precisionByColumn.TryGetValue(column, out int precision) ? precision : 0;
    }

    private RowKey CreateModelKey(List<string> row) {
        var items = new List<RowKeyItem>();
        for(int index = 0; index < _header.Count; index++) {
            string value = index < row.Count ? row[index] : "";
            double? number = ExcelText.ParseNumber(value);
            items.Add(number != null
                ? new RowKeyItem { IsNumber = true, Number = PythonFormat.Round(number.Value, GetPrecision(_header[index])) }
                : new RowKeyItem { Text = ExcelText.Normalize(value) });
        }

        return new RowKey(items);
    }

    private RowKey CreateExcelKey(int rowNumber, Dictionary<string, SheetCell?> cells) {
        var items = new List<RowKeyItem>();
        foreach(string column in _header) {
            cells.TryGetValue(column, out SheetCell? cell);
            string value = cell != null ? cell.Text : "";
            double? number = ExcelText.ParseNumber(value);
            if(column == _quantityColumn && cell != null && cell.IsText) {
                _textQuantities.Add((rowNumber, value));
            }

            // целое в столбце без дробных значений в модели сравнивается как число, дробное — как текст
            bool compareAsNumber = number != null
                                   && (GetPrecision(column) != 0
                                       || number.Value == Math.Truncate(number.Value)
                                       || column == _quantityColumn);
            items.Add(compareAsNumber
                ? new RowKeyItem { IsNumber = true, Number = PythonFormat.Round(number.GetValueOrDefault(), GetPrecision(column)) }
                : new RowKeyItem { Text = ExcelText.Normalize(value) });
        }

        return new RowKey(items);
    }

    private RowKeyBag SeparateInsulationRows(
        ScheduleTableData insulationSchedule,
        RowKeyBag extraInExcel,
        string fileName,
        FindingWriter writer) {

        (List<string>? insulationHeader, List<List<string>> insulationRows) = ReadModelTable(insulationSchedule);
        if(insulationHeader == null || !insulationHeader.SequenceEqual(_header)) {
            return extraInExcel;
        }

        var insulationBag = new RowKeyBag();
        foreach(List<string> row in insulationRows) {
            insulationBag.Add(CreateModelKey(row));
        }

        RowKeyBag fromInsulation = RowKeyBag.Intersect(extraInExcel, insulationBag);
        if(fromInsulation.IsEmpty) {
            return extraInExcel;
        }

        writer.Add(
            fileName + ": на листе «Исходная ВОР» есть " + fromInsulation.TotalCount + " строк спецификации "
            + "«ВОР_МТР_ТИП_КР_Изоляция» (изоляция должна выдаваться отдельным файлом *ВОР_Изоляция.xlsx).",
            FindingKinds.Clarify);
        return RowKeyBag.Subtract(extraInExcel, fromInsulation);
    }

    private string GetDisplayValue(RowKey key, string column) {
        int index = _header.IndexOf(column);
        RowKeyItem item = index >= 0 ? key.Items[index] : new RowKeyItem { Text = "" };
        if(item.IsNumber) {
            return PythonFormat.GeneralFormat(item.Number).Replace(".", ",");
        }

        return string.IsNullOrEmpty(item.Text) ? "—" : item.Text;
    }

    /// <summary>
    /// Пары «строка модели — строка Excel»: сначала с тем же содержимым кроме количества,
    /// затем с теми же видимыми столбцами; оставшиеся — строки только в модели или только в Excel.
    /// </summary>
    private List<List<string>> DescribeDifferences(List<RowKey> missingInExcel, List<RowKey> extraInExcel) {
        int quantityIndex = _header.IndexOf(_quantityColumn);
        var displayColumns = _displayColumns.Where(_header.Contains).ToList();

        string WithoutQuantity(RowKey key) {
            return string.Join("\u0002", key.Code.Split('\u0002').Where((_, index) => index != quantityIndex));
        }

        string DisplayKey(RowKey key) {
            return string.Join("\u0003", displayColumns.Select(column => GetDisplayValue(key, column)));
        }

        List<string> DisplayValues(RowKey key) {
            return displayColumns.Select(column => GetDisplayValue(key, column)).ToList();
        }

        var differences = new List<List<string>>();
        var extraByContent = new OrderedMap<string, List<RowKey>>();
        foreach(RowKey key in extraInExcel) {
            extraByContent.GetOrAdd(WithoutQuantity(key), () => new List<RowKey>()).Add(key);
        }

        var unmatchedModelRows = new List<RowKey>();
        foreach(RowKey modelRow in missingInExcel) {
            if(extraByContent.TryGetValue(WithoutQuantity(modelRow), out List<RowKey> candidates) && candidates.Count > 0) {
                RowKey excelRow = PopLast(candidates);
                differences.Add(DisplayValues(modelRow)
                    .Concat(new[] {
                        GetDisplayValue(modelRow, _quantityColumn), GetDisplayValue(excelRow, _quantityColumn),
                        "другое количество"
                    })
                    .ToList());
            } else {
                unmatchedModelRows.Add(modelRow);
            }
        }

        var extraByDisplay = new OrderedMap<string, List<RowKey>>();
        foreach(RowKey key in extraByContent.Values.SelectMany(rows => rows)) {
            extraByDisplay.GetOrAdd(DisplayKey(key), () => new List<RowKey>()).Add(key);
        }

        var onlyInModel = new List<RowKey>();
        foreach(RowKey modelRow in unmatchedModelRows) {
            if(extraByDisplay.TryGetValue(DisplayKey(modelRow), out List<RowKey> candidates) && candidates.Count > 0) {
                RowKey excelRow = PopLast(candidates);
                var otherDifferences = _header
                    .Where(column => !displayColumns.Contains(column)
                                     && column != _quantityColumn
                                     && GetDisplayValue(modelRow, column) != GetDisplayValue(excelRow, column))
                    .Select(column => column + ": " + GetDisplayValue(modelRow, column) + " / "
                                      + GetDisplayValue(excelRow, column));
                differences.Add(DisplayValues(modelRow)
                    .Concat(new[] {
                        GetDisplayValue(modelRow, _quantityColumn), GetDisplayValue(excelRow, _quantityColumn),
                        "отличаются: " + string.Join("; ", otherDifferences)
                    })
                    .ToList());
            } else {
                onlyInModel.Add(modelRow);
            }
        }

        foreach(RowKey modelRow in onlyInModel) {
            differences.Add(DisplayValues(modelRow)
                .Concat(new[] { GetDisplayValue(modelRow, _quantityColumn), "нет", "строки нет в Excel" })
                .ToList());
        }

        foreach(RowKey excelRow in extraByDisplay.Values.SelectMany(rows => rows)) {
            differences.Add(DisplayValues(excelRow)
                .Concat(new[] { "нет", GetDisplayValue(excelRow, _quantityColumn), "строки нет в модели" })
                .ToList());
        }

        // по первым четырём ячейкам; в строке их может быть меньше, если видимых столбцов нет
        return differences
            .OrderBy(row => GetCell(row, 0), StringComparer.Ordinal)
            .ThenBy(row => GetCell(row, 1), StringComparer.Ordinal)
            .ThenBy(row => GetCell(row, 2), StringComparer.Ordinal)
            .ThenBy(row => GetCell(row, 3), StringComparer.Ordinal)
            .ToList();
    }

    private static string GetCell(List<string> row, int index) {
        return index < row.Count ? row[index] : "";
    }

    private static RowKey PopLast(List<RowKey> keys) {
        RowKey last = keys[keys.Count - 1];
        keys.RemoveAt(keys.Count - 1);
        return last;
    }
}
