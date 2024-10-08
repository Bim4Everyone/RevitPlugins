using System;
using System.Drawing;
using System.Runtime.InteropServices;

using Autodesk.Revit.UI;

using Microsoft.Office.Interop.Excel;

using DataTable = System.Data.DataTable;

namespace RevitDeclarations.Models {
    internal class ExcelExporter : ITableExporter {
        private readonly Color _apartInfoColor = Color.FromArgb(221, 235, 247);
        private readonly Color _mainRoomsColor = Color.FromArgb(248, 203, 173);
        private readonly Color _summerRoomsColor = Color.FromArgb(217, 235, 205);
        private readonly Color _nonConfigRoomsColor = Color.FromArgb(237, 237, 237);
        private readonly Color _utpColor = Color.FromArgb(226, 207, 245);

        public void Export(string path, IDeclarationDataTable declarationTable) {
            /* Releasing all COM objects was made on the basis of the article:
             * https://www.add-in-express.com/creating-addins-blog/release-excel-com-objects/
             */
            Type officeType = Type.GetTypeFromProgID("Excel.Application.16");

            if(officeType == null) {
                TaskDialog.Show("Ошибка", "Excel 2016 не найден на компьютере");
                return;
            }

            Application excelApp = new Application {
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
                workSheet = (Worksheet) workSheets["Лист1"];

                DataTable headerTable = declarationTable.HeaderDataTable;
                for(int i = 0; i < headerTable.Columns.Count; i++) {
                    workSheet.Cells[1, i + 1] = headerTable.Rows[0][i];
                }

                DataTable mainTable = declarationTable.MainDataTable;
                for(int i = 0; i < mainTable.Rows.Count; i++) {
                    for(int j = 0; j < mainTable.Columns.Count; j++) {
                        workSheet.Cells[i + 2, j + 1] = mainTable.Rows[i][j];
                    }
                }

                //SetGraphicSettings(workSheet, declarationTable.TableInfo);

                workBook.SaveAs(path);
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

        private void SetGraphicSettings(Worksheet workSheet, ApartDeclTableInfo tableInfo) {
            workSheet.StandardWidth = 12;
            Range range = (Range) workSheet.Rows[1];

            range.RowHeight = 60;
            range.WrapText = true;

            Microsoft.Office.Interop.Excel.Font font = range.Font;
            font.Bold = true;

            ((Range) workSheet.Columns[1]).NumberFormat = "@";

            Range firstCell = (Range) workSheet.Cells[1, 1];
            Range lastCell = (Range) workSheet.Cells[tableInfo.Apartments.Count + 1, tableInfo.FullTableWidth];

            workSheet.Range[firstCell, lastCell].Borders.ColorIndex = 0;
            workSheet.Range[firstCell, lastCell].HorizontalAlignment = XlHAlign.xlHAlignCenter;
            workSheet.Range[firstCell, lastCell].VerticalAlignment = XlVAlign.xlVAlignCenter;

            for(int i = 1; i <= tableInfo.FullTableWidth; i++) {
                if(i <= ApartDeclTableInfo.InfoWidth) {
                    ((Range) workSheet.Columns[i]).ColumnWidth = 15.5;
                    ((Range) workSheet.Cells[1, i]).Interior.Color = _apartInfoColor;
                } else if(i > ApartDeclTableInfo.InfoWidth && i <= tableInfo.SummerRoomsStart) {
                    ((Range) workSheet.Columns[i]).ColumnWidth = 10;
                    ((Range) workSheet.Cells[1, i]).Interior.Color = _mainRoomsColor;

                    int checkColumnNumber = (i - ApartDeclTableInfo.InfoWidth) % 3;
                    if(checkColumnNumber == 0) {
                        ((Range) workSheet.Columns[i - 2]).NumberFormat = "@";
                        ((Range) workSheet.Columns[i - 1]).ColumnWidth = 17;
                    }
                } else if(i > tableInfo.SummerRoomsStart && i <= tableInfo.OtherRoomsStart) {
                    ((Range) workSheet.Columns[i]).ColumnWidth = 10;
                    ((Range) workSheet.Cells[1, i]).Interior.Color = _summerRoomsColor;

                    int checkColumnNumber = (i - tableInfo.SummerRoomsStart) % 4;
                    if(checkColumnNumber == 0) {
                        ((Range) workSheet.Columns[i - 3]).NumberFormat = "@";
                        ((Range) workSheet.Columns[i - 2]).ColumnWidth = 17;
                    }
                } else if(i > tableInfo.OtherRoomsStart && i <= tableInfo.UtpStart) {
                    ((Range) workSheet.Columns[i]).ColumnWidth = 10;
                    ((Range) workSheet.Cells[1, i]).Interior.Color = _nonConfigRoomsColor;

                    int checkColumnNumber = (i - tableInfo.OtherRoomsStart) % 3;
                    if(checkColumnNumber == 0) {
                        ((Range) workSheet.Columns[i - 2]).NumberFormat = "@";
                        ((Range) workSheet.Columns[i - 1]).ColumnWidth = 17;
                    }
                } else {
                    ((Range) workSheet.Columns[i]).ColumnWidth = 12;
                    ((Range) workSheet.Cells[1, i]).Interior.Color = _utpColor;
                }
            }
        }
    }
}
