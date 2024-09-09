using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    internal class ExcelExportViewModel : ExportViewModel {
        public ExcelExportViewModel(string name) : base(name) {
        }

        public override void Export(string path,
                                    IEnumerable<Apartment> apartments,
                                    DeclarationSettings settings) {
            DeclarationTableInfo tableData = new DeclarationTableInfo(apartments.ToList(), settings);
            DeclarationDataTable table = new DeclarationDataTable(tableData, settings);

            ExcelExporter exporter = new ExcelExporter();
            exporter.Export(path, table);
            TaskDialog.Show("Декларации", "Файл Excel создан");
        }
    }
}
