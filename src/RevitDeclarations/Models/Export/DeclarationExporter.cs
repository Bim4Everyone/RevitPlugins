using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Autodesk.Revit.UI;

using Microsoft.Office.Interop.Excel;

namespace RevitDeclarations.Models {
    internal class DeclarationExporter {
        private readonly DeclarationSettings _settings;

        public DeclarationExporter(DeclarationSettings settings) {
            _settings = settings;
        }

        public void ExportToExcel(string path, ExcelTableData tableData) {
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

                new ExcelTableCreator(tableData, _settings).Create(workSheet);
                TaskDialog.Show("Декларации", "Файл Excel создан");

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

        public void ExportToJson(string path, IEnumerable<Apartment> apartments) {
            JsonExporter<Apartment> exporter = new JsonExporter<Apartment>();
            exporter.Export(path, apartments);
            TaskDialog.Show("Декларации", "Файл JSON создан");
        }
    }
}
