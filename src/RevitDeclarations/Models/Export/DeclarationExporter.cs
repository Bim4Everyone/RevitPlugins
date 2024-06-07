using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using Autodesk.Revit.DB;

using Microsoft.Office.Interop.Excel;

using pyRevitLabs.Json;

namespace RevitDeclarations.Models {
    internal class DeclarationExporter {
        private readonly DeclarationSettings _settings;

        public DeclarationExporter(DeclarationSettings settings) {
            _settings = settings;
        }

        public void ExportToExcel(string path, DeclarationTableData tableData) {
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

                    new DeclarationTableCreator(tableData, _settings).Create(workSheet);

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

        public void ExportToJson(string path, List<Apartment> apartments) {
            path = path + ".json";
            using(StreamWriter file = File.CreateText(path)) {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, apartments);
            }
        }
    }
}
