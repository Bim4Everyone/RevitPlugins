using System;
using System.Collections.Generic;

using Autodesk.Revit.UI;

using RevitDeclarations.Models.Export.Exporters;

namespace RevitDeclarations.Models {
    internal class DeclarationExporter {
        private readonly DeclarationSettings _settings;

        public DeclarationExporter(DeclarationSettings settings) {
            _settings = settings;
        }

        public void ExportToExcel(string path, ExcelTableData tableData) {
            try {
                ExcelExporter excel = new ExcelExporter();
                excel.Export(path, tableData, _settings);
                TaskDialog.Show("Декларации", "Файл Excel создан");
            } catch(Exception e) {
                var taskDialog = new TaskDialog("Ошибка выгрузки") {
                    CommonButtons = TaskDialogCommonButtons.No | TaskDialogCommonButtons.Yes,
                    MainContent = "Произошла ошибка Excel.\nВыгрузить декларацию в формате csv?",
                    ExpandedContent = $"Описание ошибки: {e.Message}"
                };

                TaskDialogResult dialogResult = taskDialog.Show();

                if(dialogResult == TaskDialogResult.Yes) {
                    ExportToCSV(path, tableData);
                }
            }
        }

        public void ExportToJson(string path, IEnumerable<Apartment> apartments) {
            JsonExporter<Apartment> exporter = new JsonExporter<Apartment>();
            exporter.Export(path, apartments);
            TaskDialog.Show("Декларации", "Файл JSON создан");
        }

        public void ExportToCSV(string path, ExcelTableData tableData) {
            CsvTableCreator csvTableCreator = new CsvTableCreator(tableData, _settings);

            CsvExporter exporter = new CsvExporter();

            string declarationData = csvTableCreator.Create();

            exporter.Export(path, declarationData);
            TaskDialog.Show("Декларации", "Файл CSV создан");
        }
    }
}
