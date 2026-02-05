using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

using Autodesk.Revit.UI;

using Microsoft.Office.Interop.Excel;

namespace RevitDeclarations.Models;
internal class ExcelExporter : ITableExporter {
    private readonly Color _apartInfoColor = Color.FromArgb(221, 235, 247);
    private readonly Color _mainRoomsColor = Color.FromArgb(248, 203, 173);
    private readonly Color _summerRoomsColor = Color.FromArgb(217, 235, 205);
    private readonly Color _nonConfigRoomsColor = Color.FromArgb(237, 237, 237);
    private readonly Color _utpColor = Color.FromArgb(226, 207, 245);

    public void Export(string path, IDeclarationDataTable declarationDataTable) {
        string fullPath = $"{path}.xlsx";
        /* Releasing all COM objects was made on the basis of the article:
         * https://www.add-in-express.com/creating-addins-blog/release-excel-com-objects/
         */
        var officeType = Type.GetTypeFromProgID("Excel.Application.16");

        if(officeType == null) {
            TaskDialog.Show("Ошибка", "Excel 2016 не найден на компьютере");
            return;
        }

        var excelApp = new Application {
            Visible = false,
            DisplayAlerts = false
        };

        Workbooks workBooks = null;
        Workbook workBook = null;
        Sheets workSheets = null;
        Worksheet workSheet = null;

        try {
            workBooks = excelApp.Workbooks;
            workBook = workBooks.Add();
            workSheets = workBook.Worksheets;
            workSheet = (Worksheet) workSheets[1];
            workSheet.Name = declarationDataTable.Name;

            SetMainSheetGraphicSettings(workSheet, declarationDataTable.TableInfo);

            var headerTable = declarationDataTable.HeaderDataTable;
            for(int i = 0; i < headerTable.Columns.Count; i++) {
                workSheet.Cells[1, i + 1] = headerTable.Rows[0][i];
            }

            var mainTable = declarationDataTable.MainDataTable;
            for(int i = 0; i < mainTable.Rows.Count; i++) {
                for(int j = 0; j < mainTable.Columns.Count; j++) {
                    workSheet.Cells[i + 2, j + 1] = mainTable.Rows[i][j];
                }
            }

            if(declarationDataTable.SubTables.Any()) {
                Worksheet subWorkSheet = null;

                foreach(var subDataTable in declarationDataTable.SubTables) {
                    subWorkSheet = (Worksheet) workSheets.Add(After: workBook.Sheets[workBook.Sheets.Count]);
                    subWorkSheet.Name = subDataTable.Name;

                    var subHeaderTable = subDataTable.HeaderDataTable;
                    for(int i = 0; i < subHeaderTable.Columns.Count; i++) {
                        subWorkSheet.Cells[1, i + 1] = subHeaderTable.Rows[0][i];
                    }

                    var subMainTable = subDataTable.MainDataTable;
                    for(int i = 0; i < subMainTable.Rows.Count; i++) {
                        for(int j = 0; j < subMainTable.Columns.Count; j++) {
                            subWorkSheet.Cells[i + 2, j + 1] = subMainTable.Rows[i][j];
                        }
                    }

                    SetSubSheetGraphicSettings(subWorkSheet);
                }
            }

            workSheet.Activate();

            workBook.SaveAs(fullPath);
            workBook.Close(false);
        } finally {
            excelApp.Quit();
            if(workSheet != null) { Marshal.ReleaseComObject(workSheet); }
            if(workSheets != null) { Marshal.ReleaseComObject(workSheets); }
            if(workBook != null) { Marshal.ReleaseComObject(workBook); }
            if(workBooks != null) { Marshal.ReleaseComObject(workBooks); }
            if(excelApp != null) { Marshal.ReleaseComObject(excelApp); }
        }
    }

    private void SetMainSheetGraphicSettings(Worksheet workSheet, ITableInfo tableInfo) {
        // Общие настройки
        workSheet.StandardWidth = 12;
        ((Range) workSheet.Columns[1]).NumberFormat = "@";

        // Настройка границ и выравнивания для всей таблицы
        var firstCell = (Range) workSheet.Cells[1, 1];
        var lastCell = (Range) workSheet.Cells[tableInfo.RoomGroups.Count + 1, tableInfo.ColumnsTotalNumber];
        workSheet.Range[firstCell, lastCell].Borders.ColorIndex = 0;
        workSheet.Range[firstCell, lastCell].HorizontalAlignment = XlHAlign.xlHAlignCenter;
        workSheet.Range[firstCell, lastCell].VerticalAlignment = XlVAlign.xlVAlignCenter;

        for(int i = 0; i < tableInfo.ColumnsTotalNumber; i++) {
            if(tableInfo.AreaTypeColumnsIndexes.Contains(i)) {
                string strFormat = $"0.{new string('0', tableInfo.Settings.AccuracyForArea)}";
                ((Range) workSheet.Columns[i + 1]).NumberFormat = strFormat;
            } else if(tableInfo.LengthTypeColumnsIndexes.Contains(i)) {
                string strFormat = $"0.{new string('0', tableInfo.Settings.AccuracyForLength)}";
                ((Range) workSheet.Columns[i + 1]).NumberFormat = strFormat;
            } else {
                ((Range) workSheet.Columns[i + 1]).NumberFormat = "@";
            }
        }

        // Настройка шапки таблицы
        var range = (Range) workSheet.Rows[1];
        range.RowHeight = 60;
        range.WrapText = true;
        var font = range.Font;
        font.Bold = true;

        // Настройка графики и форматов по столбцам
        for(int i = 1; i <= tableInfo.ColumnsTotalNumber; i++) {
            // Общая информация про группы помещений
            if(i <= tableInfo.GroupsInfoColumnsNumber) {
                ((Range) workSheet.Columns[i]).ColumnWidth = 15.5;
                ((Range) workSheet.Cells[1, i]).Interior.Color = _apartInfoColor;
                // Основные помещения квартир
            } else if(i > tableInfo.GroupsInfoColumnsNumber && i <= tableInfo.SummerRoomsStart) {
                ((Range) workSheet.Columns[i]).ColumnWidth = 10;
                ((Range) workSheet.Cells[1, i]).Interior.Color = _mainRoomsColor;

                int checkColumnNumber = (i - tableInfo.GroupsInfoColumnsNumber) % 3;
                if(checkColumnNumber == 0) {
                    //((Range) workSheet.Columns[i - 2]).NumberFormat = "@";
                    ((Range) workSheet.Columns[i - 1]).ColumnWidth = 17;
                }
                // Летние помещения квартир
            } else if(i > tableInfo.SummerRoomsStart && i <= tableInfo.OtherRoomsStart) {
                ((Range) workSheet.Columns[i]).ColumnWidth = 10;
                ((Range) workSheet.Cells[1, i]).Interior.Color = _summerRoomsColor;

                int checkColumnNumber = (i - tableInfo.SummerRoomsStart) % 4;
                if(checkColumnNumber == 0) {
                    //((Range) workSheet.Columns[i - 3]).NumberFormat = "@";
                    ((Range) workSheet.Columns[i - 2]).ColumnWidth = 17;
                }
                // Остальные (не из списка приоритетов) помещения квартир
            } else if(i > tableInfo.OtherRoomsStart && i <= tableInfo.UtpStart) {
                ((Range) workSheet.Columns[i]).ColumnWidth = 10;
                ((Range) workSheet.Cells[1, i]).Interior.Color = _nonConfigRoomsColor;

                int checkColumnNumber = (i - tableInfo.OtherRoomsStart) % 3;
                if(checkColumnNumber == 0) {
                    //((Range) workSheet.Columns[i - 2]).NumberFormat = "@";
                    ((Range) workSheet.Columns[i - 1]).ColumnWidth = 17;
                }
                // УТП квартир
            } else {
                ((Range) workSheet.Columns[i]).ColumnWidth = 12;
                ((Range) workSheet.Cells[1, i]).Interior.Color = _utpColor;
            }
        }
    }

    private void SetSubSheetGraphicSettings(Worksheet workSheet) {
        var range = (Range) workSheet.Rows[1];
        var font = range.Font;
        font.Bold = true;

        ((Range) workSheet.Columns[1]).ColumnWidth = 43;
        ((Range) workSheet.Columns[2]).ColumnWidth = 43;
        ((Range) workSheet.Columns[3]).ColumnWidth = 17;

        ((Range) workSheet.Columns[2]).NumberFormat = "0.0";

        ((Range) workSheet.Rows[1]).HorizontalAlignment = XlHAlign.xlHAlignCenter;
        ((Range) workSheet.Columns[2]).HorizontalAlignment = XlHAlign.xlHAlignCenter;
    }
}
