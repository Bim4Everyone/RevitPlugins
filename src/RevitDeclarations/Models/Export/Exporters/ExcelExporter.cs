using System;
using System.Runtime.InteropServices;

using Autodesk.Revit.UI;

using Microsoft.Office.Interop.Excel;

namespace RevitDeclarations.Models.Export.Exporters {
    internal class ExcelExporter {
        public void Export(string path, ExcelTableData tableData, DeclarationSettings settings) {
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

                new ExcelTableCreator(tableData, settings).Create(workSheet);
                
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
    }
}
