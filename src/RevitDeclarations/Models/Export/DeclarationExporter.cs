using System;
using System.Runtime.InteropServices;

using Microsoft.Office.Interop.Excel;

namespace RevitDeclarations.Models {
    internal class DeclarationExporter {
        private readonly DeclarationTableData _tableData;
        private readonly DeclarationSettings _settings;

        public DeclarationExporter(DeclarationTableData tableData, DeclarationSettings settings) {
            _tableData = tableData;
            _settings = settings;
        }

        public void ExportToExcel(string path) {
            /* Releasing all COM objects was made on the basis of the article:
             * https://www.add-in-express.com/creating-addins-blog/release-excel-com-objects/
             */
            Type officeType = Type.GetTypeFromProgID("Excel.Application");

            if(officeType != null) {
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
                    workSheet = workSheets["Лист1"];

                    new DeclarationTableCreator(_tableData, _settings).Create(workSheet);

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

        public static void ExportToJson() {

        }
    }
}
