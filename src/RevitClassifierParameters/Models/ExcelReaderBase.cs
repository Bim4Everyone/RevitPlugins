using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using dosymep.SimpleServices;

using Microsoft.Office.Interop.Excel;

namespace RevitClassifierParameters.Models;

/// <summary>
/// Базовый класс для чтения данных из Excel-файла через COM-интероп.
/// Инкапсулирует открытие книги, чтение диапазона первого листа
/// и корректное освобождение COM-объектов.
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
        Application excel = null;
        Workbook workbook = null;
        Worksheet worksheet = null;
        Range range = null;

        try {
            excel = new Application {
                Visible = false,
                DisplayAlerts = false,
                ScreenUpdating = false
            };
            workbook = excel.Workbooks.Open(path, ReadOnly: true);
            worksheet = (Worksheet) workbook.Worksheets[1];

            int lastRow = GetLastRow(worksheet);
            if(lastRow < FirstDataRow) {
                _messageBoxService.Show(_localizationService.GetLocalizedString(EmptySheetMessageKey));
                return default;
            }

            range = worksheet.Range[
                worksheet.Cells[FirstDataRow, 1],
                worksheet.Cells[lastRow, ColumnCount]];

            if(range.Value2 is not object[,] values) {
                _messageBoxService.Show(_localizationService.GetLocalizedString(EmptySheetMessageKey));
                return default;
            }

            return BuildResult(values);
        } finally {
            workbook?.Close(false);
            excel?.Quit();

            Release(range);
            Release(worksheet);
            Release(workbook);
            Release(excel);
        }
    }

    /// <summary>
    /// Строит результат разбора из прочитанного двумерного массива значений ячеек.
    /// Индексация массива начинается с 1 по обоим измерениям.
    /// </summary>
    protected abstract TResult BuildResult(object[,] rows);

    protected static string GetString(object value) {
        return value?.ToString()?.Trim();
    }

    private int GetLastRow(Worksheet worksheet) {
        var lastCell = worksheet.Cells.Find(
            "*",
            Type.Missing,
            Type.Missing,
            Type.Missing,
            XlSearchOrder.xlByRows,
            XlSearchDirection.xlPrevious,
            false);
        try {
            return lastCell?.Row ?? 1;
        } finally {
            Release(lastCell);
        }
    }

    private static void Release(object comObject) {
        if(comObject != null && Marshal.IsComObject(comObject)) {
            Marshal.ReleaseComObject(comObject);
        }
    }
}
