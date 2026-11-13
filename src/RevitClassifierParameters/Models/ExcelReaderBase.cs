using System.IO;

using ClosedXML.Excel;

using dosymep.SimpleServices;

namespace RevitClassifierParameters.Models;

/// <summary>
/// Базовый класс для чтения данных из Excel-файла (.xlsx) через ClosedXML.
/// Инкапсулирует открытие книги и чтение диапазона первого листа
/// в двумерный массив значений ячеек.
/// </summary>
/// <typeparam name="TResult">Тип результата разбора данных листа.</typeparam>
internal abstract class ExcelReaderBase<TResult> {
    protected readonly ILocalizationService _localizationService;
    protected readonly IMessageBoxService _messageBoxService;

    protected ExcelReaderBase(
        ILocalizationService localizationService,
        IMessageBoxService messageBoxService) {
        _localizationService = localizationService;
        _messageBoxService = messageBoxService;
    }

    /// <summary>
    /// Номер первой строки с данными (строки выше считаются заголовком).
    /// </summary>
    protected abstract int FirstDataRow { get; }

    /// <summary>
    /// Количество считываемых столбцов, начиная с первого.
    /// </summary>
    protected abstract int ColumnCount { get; }

    /// <summary>
    /// Ключ локализации сообщения о пустом листе/файле.
    /// </summary>
    protected abstract string EmptySheetMessageKey { get; }

    public TResult Read(string path) {
        using var stream = File.OpenRead(path);
        using var workbook = new XLWorkbook(stream);

        var worksheet = workbook.Worksheet(1);

        var lastUsedRow = worksheet.LastRowUsed(XLCellsUsedOptions.AllContents);
        int lastRow = lastUsedRow?.RowNumber() ?? 0;

        if(lastRow < FirstDataRow) {
            _messageBoxService.Show(_localizationService.GetLocalizedString(EmptySheetMessageKey));
            return default;
        }

        var values = ReadValues(worksheet, lastRow);
        return BuildResult(values);
    }

    /// <summary>
    /// Строит результат разбора из прочитанного двумерного массива значений ячеек.
    /// Индексация массива начинается с 0 по обоим измерениям:
    /// первое измерение — строки данных (0..rowCount-1),
    /// второе — столбцы (0..ColumnCount-1).
    /// </summary>
    protected abstract TResult BuildResult(object[,] rows);

    protected static string GetString(object value) {
        return value?.ToString()?.Trim();
    }

    /// <summary>
    /// Читает диапазон данных листа в двумерный массив с 0-based индексацией.
    /// Размерности массива равны фактическому числу строк данных и <see cref="ColumnCount"/>.
    /// </summary>
    private object[,] ReadValues(IXLWorksheet worksheet, int lastRow) {
        int rowCount = lastRow - FirstDataRow + 1;
        var values = new object[rowCount, ColumnCount];

        for(int r = 0; r < rowCount; r++) {
            int sheetRow = FirstDataRow + r;
            for(int c = 0; c < ColumnCount; c++) {
                var cell = worksheet.Cell(sheetRow, c + 1);
                values[r, c] = GetCellValue(cell);
            }
        }
        return values;
    }

    private static string GetCellValue(IXLCell cell) {
        return cell.IsEmpty() ? null : cell.GetString();
    }
}
