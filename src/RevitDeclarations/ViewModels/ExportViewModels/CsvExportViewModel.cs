using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.UI;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    internal class CsvExportViewModel : ExportViewModel {
        public CsvExportViewModel(string name, DeclarationSettings settings) 
            : base(name, settings) { 
        }

        public void ExportTable(string path, DeclarationDataTable table) {
            CsvExporter exporter = new CsvExporter();
            exporter.Export(path, table);
            TaskDialog.Show("Декларации", "Файл CSV создан");
        }

        public override void Export(string path, IEnumerable<Apartment> apartments) {
            DeclarationTableInfo tableData = new DeclarationTableInfo(apartments.ToList(), _settings);
            DeclarationDataTable table = new DeclarationDataTable(tableData, _settings);
            ExportTable(path, table);
        }
    }
}
