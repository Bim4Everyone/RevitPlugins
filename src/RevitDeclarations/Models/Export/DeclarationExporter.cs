using System;
using System.Collections.Generic;

using Autodesk.Revit.UI;

namespace RevitDeclarations.Models {
    internal class DeclarationExporter {
        public void ExportToExcel(string path, DeclarationDataTable table) {
            ExcelExporter exporter = new ExcelExporter();
            exporter.Export(path, table);
            TaskDialog.Show("Декларации", "Файл Excel создан");
        }

        public void ExportToCSV(string path, DeclarationDataTable table) {
            CsvExporter exporter = new CsvExporter();
            exporter.Export(path, table);
            TaskDialog.Show("Декларации", "Файл CSV создан");
        }

        public void ExportToJson(string path, IEnumerable<Apartment> apartments) {
            JsonExporter<Apartment> exporter = new JsonExporter<Apartment>();
            exporter.Export(path, apartments);
            TaskDialog.Show("Декларации", "Файл JSON создан");
        }
    }
}
