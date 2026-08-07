using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using dosymep.SimpleServices;

using Microsoft.Office.Interop.Excel;

namespace RevitClassifierParameters.Models.FacadeType;

/// <summary>
/// Читает Excel-файл с правилами заполнения типа фасада.
/// Ожидается три столбца:
/// 1 - характеристика функции (со скобками, например "(ФМ)");
/// 2 - сокращение основного материала (например "ОК");
/// 3 - значение для записи в параметр.
/// Первая строка — заголовок, данные начинаются со второй строки.
/// </summary>
public class FacadeTypeExcelReader {
    private const int _firstDataRow = 2;

    private readonly ILocalizationService _localizationService;
    private readonly IMessageBoxService _messageBoxService;

    public FacadeTypeExcelReader(
        ILocalizationService localizationService,
        IMessageBoxService messageBoxService) {
        _localizationService = localizationService;
        _messageBoxService = messageBoxService;
    }

    public List<FacadeTypeItem> Read(string path) {
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
            if(lastRow < _firstDataRow) {
                _messageBoxService.Show("Файл правил заполнения типа фасада пуст или не содержит данных.");
                return null;
            }

            range = worksheet.Range[
                worksheet.Cells[_firstDataRow, 1],
                worksheet.Cells[lastRow, 3]];

            if(range.Value2 is not object[,] values) {
                _messageBoxService.Show("Файл правил заполнения типа фасада пуст или не содержит данных.");
                return null;
            }

            return BuildItems(values);
        } finally {
            workbook?.Close(false);
            excel?.Quit();

            Release(range);
            Release(worksheet);
            Release(workbook);
            Release(excel);

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    private List<FacadeTypeItem> BuildItems(object[,] rows) {
        var result = new List<FacadeTypeItem>();
        
        int rowCount = rows.GetLength(0);

        for(int row = 1; row <= rowCount; row++) {
            string function = GetString(rows[row, 1]);
            string material = GetString(rows[row, 2]);
            string value = GetString(rows[row, 3]);

            if(string.IsNullOrWhiteSpace(function)
               || string.IsNullOrWhiteSpace(material)
               || string.IsNullOrWhiteSpace(value)) {
                continue;
            }

            result.Add(new FacadeTypeItem {
                FunctionCharacteristic = Normalize(function),
                MaterialAbbreviation = Normalize(material),
                Value = value
            });
        }
        return result;
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

    private string GetString(object value) {
        return value?.ToString()?.Trim();
    }

    /// <summary>
    /// Приводит ключевое поле правила (характеристику функции или сокращение материала)
    /// к единому виду для последующего сравнения с распарсенным именем типоразмера стены.
    /// </summary>
    private string Normalize(string value) {
        return (value ?? string.Empty).Trim().ToUpperInvariant();
    }

    private void Release(object comObject) {
        if(comObject != null && Marshal.IsComObject(comObject))
            Marshal.ReleaseComObject(comObject);
    }
}
